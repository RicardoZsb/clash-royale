import json,tempfile,unittest
from pathlib import Path
from game import Game,GameError
from import_config import from_xlsx

class CountingRandom:
    def __init__(self,values):self.values=iter(values);self.calls=0
    def random(self):self.calls+=1;return next(self.values)
class RewardTests(unittest.TestCase):
    def setUp(self):
        self.temp=tempfile.TemporaryDirectory();self.path=Path(self.temp.name)/'test.sqlite'
        self.rng=CountingRandom([0,.99,0,0]);self.game=Game(self.path,rng=self.rng)
        self.config=from_xlsx(Path(__file__).parent.parent/'Assets/Tables/ClashRoyale_V5.4.xlsx');self.game.install(self.config,'test');self.game.create_player()
        with self.game.tx() as db:
            c=self.game.config(db)
            for name in ['one','two']:db.execute('INSERT INTO chests(box_id,player_id,bundle_id,chest_id,initial_star,created_at) VALUES(?,?,?,?,?,?)',(name,'P001',c['_bundle'],'600001',1,self.game.now()))
    def tearDown(self):self.temp.cleanup()
    def command(self,key,operation,data=None):return self.game.command('P001',key,operation,data)
    def test_four_rolls_resume_no_early_grant_and_separate_boxes(self):
        gold=self.game.state('P001')['player']['gold']
        for step,star in [(1,2),(2,2),(3,3)]:
            r=self.command('r'+str(step),'open_chest',{'box_id':'one','step':step});self.assertEqual(r['final_star'],star);self.assertFalse(r['opened'])
            self.assertEqual(self.game.state('P001')['player']['gold'],gold)
        self.game=Game(self.path,rng=self.rng)
        self.assertEqual(self.game.state('P001')['chests'][0]['roll_count'],3)
        self.command('repeat','open_chest',{'box_id':'one','step':3});self.assertEqual(self.rng.calls,3)
        r=self.command('r4','open_chest',{'box_id':'one','step':4});self.assertTrue(r['opened']);self.assertEqual(r['final_star'],4)
        once=self.game.state('P001');self.assertEqual([b['box_id'] for b in once['chests']],['two']);self.assertGreater(once['player']['gold'],gold)
        self.command('again','open_chest',{'box_id':'one','step':4});self.assertEqual(self.rng.calls,4);self.assertEqual(self.game.state('P001')['player']['gold'],once['player']['gold'])
    def test_steps_cannot_be_skipped(self):
        with self.assertRaises(GameError):self.command('skip','open_chest',{'box_id':'one','step':4})
        self.assertEqual(self.rng.calls,0)
    def test_single_pass_claim_returns_granted_item_and_persists_once(self):
        with self.game.tx() as db:db.execute('UPDATE players SET pass_xp=10')
        before=self.game.state('P001')['player']['gems']
        result=self.command('single-gems','claim_pass',{'level':2,'track':'FREE'})
        self.assertEqual(result['reward']['resource'],'gems');self.assertEqual(result['reward']['amount'],10)
        self.assertEqual(self.command('single-gems','claim_pass',{'level':2,'track':'FREE'}),result)
        again=self.command('second-click','claim_pass',{'level':2,'track':'FREE'})
        self.assertTrue(again['already_claimed']);self.assertNotIn('reward',again)
        self.game=Game(self.path)
        self.assertEqual(self.game.state('P001')['player']['gems'],before+10)
        with self.game.tx() as db:
            self.assertEqual(db.execute("SELECT COUNT(*) FROM pass_claims WHERE level=2 AND track='FREE'").fetchone()[0],1)
        with self.assertRaises(GameError):self.command('future','claim_pass',{'level':3,'track':'FREE'})
    def test_unlock_persists_and_rewards_match_resources(self):
        with self.game.tx() as db:db.execute('UPDATE players SET pass_xp=200')
        with self.assertRaises(GameError):self.command('locked','claim_pass',{'level':1,'track':'PREMIUM'})
        before=self.game.state('P001')['player'];self.command('unlock','unlock_pass');self.command('unlock-again','unlock_pass')
        self.game=Game(self.path);self.assertEqual(self.game.state('P001')['player']['premium'],1)
        self.command('common','claim_pass',{'level':1,'track':'PREMIUM'});self.command('common-again','claim_pass',{'level':1,'track':'PREMIUM'})
        self.assertEqual(self.game.state('P001')['items'],[{'resource':'common','amount':250}])
        self.command('gems','claim_pass',{'level':2});self.assertEqual(self.game.state('P001')['player']['gems'],before['gems']+10)
        self.command('chest','claim_pass',{'level':3});self.assertEqual(len(self.game.state('P001')['chests']),3)
    def test_pass_variety_and_forty_levels(self):
        rewards={r['reward_id']:r for r in self.config['rewards']}
        for track in ['free_reward_id','premium_reward_id']:
            rows=[rewards[level[track]] for level in self.config['pass_levels']];self.assertEqual(len(rows),40)
            kinds={r['resource'] or ('chest' if r['chest_star'] else 'ap' if r['ap'] else 'gold') for r in rows}
            self.assertGreaterEqual(len(kinds),7);self.assertLess(sum(r['chest_star']>0 for r in rows),12)
        self.assertEqual(rewards[self.config['pass_levels'][-1]['free_reward_id']]['chest_star'],5)
if __name__=='__main__':unittest.main()
