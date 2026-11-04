import copy,concurrent.futures,hashlib,json,tempfile,threading,unittest,urllib.request,urllib.error
from pathlib import Path
from http.server import ThreadingHTTPServer
from game import Game,GameError,dumps
from import_config import validate,from_xlsx
from server import handler

ROOT=Path(__file__).resolve().parent
class NoUpgrade:
    def random(self):return .99
class Tests(unittest.TestCase):
    def setUp(self):
        self.temp=tempfile.TemporaryDirectory();self.time=1793520000;self.g=Game(Path(self.temp.name)/'test.sqlite',lambda:self.time,NoUpgrade());self.config=json.loads((ROOT/'Config/design-seed.json').read_text(encoding='utf-8'));self.g.install(self.config,'test fixture');self.g.create_player(case=True)
    def tearDown(self):self.temp.cleanup()
    def cmd(self,op,data=None,key=None):return self.g.command('P001',key or hashlib.sha256(dumps([op,data,self.g.now()]).encode()).hexdigest(),op,data)
    def fixture_task(self,task_id='112001',id='A001'):
        t=next(t for t in self.config['tasks'] if t['task_id']==task_id)
        with self.g.tx() as db:
            cfg=self.g.config(db)
            db.execute('INSERT INTO task_instances(instance_id,player_id,task_id,bundle_id,season_id,week_id,status,accepted_at,config_snapshot) VALUES(?,?,?,?,?,?,?,?,?)',(id,'P001',task_id,cfg['_bundle'],cfg['season_id'],self.g.period()[0],'accepted',self.g.now(),dumps(t)))
        return id
    def boxes(self,count):
        with self.g.tx() as db:
            cfg=self.g.config(db)
            for i in range(count):db.execute('INSERT INTO chests(box_id,player_id,bundle_id,chest_id,initial_star,created_at) VALUES(?,?,?,?,?,?)',('SEED'+str(i),'P001',cfg['_bundle'],'600002',2,self.g.now()))
    def open_box(self,box):
        for step in range(1,5):result=self.cmd('open_chest',{'box_id':box,'step':step},box+'-roll-'+str(step))
        return result
    def facts(self,**changes):
        f={'mode':'ladder','won':True,'deck':{'cards':['c'+str(i) for i in range(8)],'saved':True,'spells':2,'non_main':True},'stats':{'towers':1}}
        f.update(changes);return f
    def settle(self,id='A001',facts=None):
        self.cmd('activate',{'instance_id':id},'activate'+id);b=self.cmd('battle_start',key='start'+id)['battle_id'];result=self.cmd('battle_settle',{'battle_id':b,'facts':facts or self.facts()},'settle'+id);return b,result
    def test_configuration_62(self):
        validate(self.config);self.assertEqual(len(self.config['tasks']),62);self.assertEqual(len({t['task_id'] for t in self.config['tasks']}),62)
        bad=copy.deepcopy(self.config);bad['tasks'][0]['chest_id']='missing'
        with self.assertRaises(ValueError):validate(bad)
    def test_excel_export_import(self):
        book=ROOT.parent/'Assets/Tables/ClashRoyale_V5.4.xlsx'
        if not book.exists():self.skipTest('Excel authoring in progress')
        imported=from_xlsx(book);self.assertEqual(len(imported['tasks']),62)
        self.assertFalse(any('source' in t for t in imported['tasks']))
    def test_daily_20_no_refill_and_accept_no_cost(self):
        before=self.g.state('P001');self.assertEqual(len(before['tasks']),20)
        for t in before['tasks']:self.cmd('accept',{'task_id':t['task_id']},'accept'+t['task_id'])
        after=self.g.state('P001');self.assertEqual(len(after['tasks']),20);self.assertEqual(after['player']['normal_ap'],5);self.assertEqual(after['player']['accepted_count'],20)
    def test_help_requires_accepted_instance(self):
        with self.assertRaises(GameError):self.cmd('help',{'instance_id':'not-accepted'})
        self.fixture_task();a=self.cmd('help',{'instance_id':'A001'},'h1');b=self.cmd('help',{'instance_id':'A001'},'h2');self.assertEqual(a,b)
    def test_canonical_case_and_idempotency(self):
        self.fixture_task();self.boxes(2);b,result=self.settle();p=self.g.state('P001')['player']
        self.assertEqual((p['normal_ap'],p['pass_xp'],p['gold'],p['pending_chests'],p['season_contribution'],p['tokens'],p['clan_score']),(3,5,1100,3,40,30,1534))
        replay=self.cmd('battle_settle',{'battle_id':b,'facts':self.facts()},'retry-other-key');self.assertEqual(result,replay)
        box=result['completed'][0]['box_id'];opened=self.open_box(box);self.assertEqual(opened['roll_count'],4);self.assertEqual(opened['final_star'],2)
        self.cmd('open_chest',{'box_id':box},'open-again');self.cmd('claim_pass',{'level':1},'claim');self.cmd('claim_pass',{'level':1},'claim-again')
        p=self.g.state('P001')['player'];self.assertEqual((p['normal_ap'],p['gold'],p['cards'],p['pending_chests']),(3,1700,15,2))
    def test_activation_capacity_concurrent(self):
        self.boxes(18);self.fixture_task('113001','a3');self.fixture_task('112001','a2');self.fixture_task('111001','a1')
        def run(i):
            try:return self.cmd('activate',{'instance_id':i},i)
            except GameError:return None
        with concurrent.futures.ThreadPoolExecutor(max_workers=3) as executor:results=list(executor.map(run,['a3','a2','a1']))
        p=self.g.state('P001')['player'];self.assertLessEqual(p['reserved_ap'],5);self.assertLessEqual(p['reserved_slots'],2);self.assertEqual(p['normal_ap'],5)
    def test_invalid_battle_releases_old_reservation_and_replans(self):
        self.fixture_task();_,r=self.settle(facts=self.facts(disconnect=True));p=self.g.state('P001')['player'];self.assertFalse(r['valid']);self.assertEqual(p['normal_ap'],5);self.assertEqual(p['reserved_ap'],2);self.assertEqual(p['pending_chests'],0)
        with self.g.tx() as db:
            self.assertEqual(db.execute("SELECT COUNT(*) FROM activations WHERE state='battle'").fetchone()[0],0)
            self.assertEqual(db.execute("SELECT COUNT(*) FROM activations WHERE state='released'").fetchone()[0],1)
    def test_transaction_rolls_back_on_late_failure(self):
        self.fixture_task();self.cmd('activate',{'instance_id':'A001'});b=self.cmd('battle_start')['battle_id']
        def fail(*_):raise GameError('Injected downstream failure')
        self.g.contribute=fail
        with self.assertRaises(GameError):self.cmd('battle_settle',{'battle_id':b,'facts':self.facts()})
        p=self.g.state('P001')['player'];self.assertEqual((p['normal_ap'],p['pass_xp'],p['pending_chests'],p['gold']),(5,3,0,1000));self.assertEqual(p['reserved_ap'],2)
    def test_withdraw_only_available_space_and_restart(self):
        with self.g.tx() as db:db.execute('UPDATE players SET normal_ap=16,reserve_ap=16')
        self.cmd('withdraw',key='w1');self.cmd('withdraw',key='w2');reopened=Game(self.g.path,lambda:self.time);p=reopened.state('P001')['player'];self.assertEqual((p['normal_ap'],p['reserve_ap']),(24,8))
    def test_request_id_conflict(self):
        self.cmd('withdraw',key='same')
        with self.assertRaises(GameError):self.cmd('buy_ap',key='same')
    def test_free_cancel_resets_at_shanghai_midnight(self):
        self.time=((self.time+8*3600)//86400+1)*86400-8*3600-1
        for n in range(3):
            self.fixture_task('112001','cancel'+str(n));self.cmd('cancel',{'instance_id':'cancel'+str(n)},'c'+str(n))
        self.time+=2
        self.fixture_task('112001','next-day');self.cmd('cancel',{'instance_id':'next-day'},'new-day-cancel')
        self.assertEqual(self.g.state('P001')['player']['gold'],1000)
    def test_locked_config_survives_reimport(self):
        self.fixture_task();self.cmd('activate',{'instance_id':'A001'});b=self.cmd('battle_start')['battle_id']
        updated=copy.deepcopy(self.config);updated['rewards'][0]['gold']=999;self.g.install(updated,'updated')
        self.cmd('battle_settle',{'battle_id':b,'facts':self.facts()});self.assertEqual(self.g.state('P001')['player']['gold'],1100)
    def test_week_change_keeps_running_tasks_and_chests(self):
        self.fixture_task();self.cmd('activate',{'instance_id':'A001'});b=self.cmd('battle_start')['battle_id'];self.time+=8*86400
        self.assertEqual(self.g.state('P001')['player']['active_count'],1)
        self.cmd('battle_settle',{'battle_id':b,'facts':self.facts()});self.assertEqual(self.g.state('P001')['player']['pending_chests'],1)
    def test_deck_order_is_not_new_deck(self):
        self.fixture_task('111013');_,r=self.settle(facts=self.facts());self.assertEqual(r['progress'][0]['progress'],1)
        self.time+=1;self.cmd('activate',{'instance_id':'A001'},'a2');b=self.cmd('battle_start',key='b2')['battle_id'];f=self.facts();f['deck']['cards'].reverse();r=self.cmd('battle_settle',{'battle_id':b,'facts':f},'s2');self.assertEqual(r['progress'][0]['progress'],1)
    def test_threshold_split(self):
        with self.g.tx() as db:db.execute('UPDATE players SET season_contribution=990')
        self.fixture_task();self.settle();p=self.g.state('P001')['player'];self.assertEqual(p['season_contribution'],1007.5);self.assertEqual(p['tokens'],22.5)
    def test_http_auth_separates_settlement(self):
        auth={'client_token':'client-test','admin_token':'admin-test'};server=ThreadingHTTPServer(('127.0.0.1',0),handler(self.g,auth,ROOT.parent/'Assets/Tables/ClashRoyale_V5.4.xlsx'));thread=threading.Thread(target=server.serve_forever,daemon=True);thread.start()
        try:
            base='http://127.0.0.1:'+str(server.server_port)
            with urllib.request.urlopen(urllib.request.Request(base+'/v1/state',headers={'Authorization':'Bearer client-test'})) as response:self.assertEqual(json.load(response)['player']['normal_ap'],5)
            req=urllib.request.Request(base+'/admin/battles/x/settle',data=b'{}',headers={'Authorization':'Bearer client-test'})
            with self.assertRaises(urllib.error.HTTPError) as err:urllib.request.urlopen(req)
            self.assertEqual(err.exception.code,401)
        finally:server.shutdown();server.server_close();thread.join()

    def test_all_62_condition_templates_can_complete(self):
        for task in self.config['tasks']:
            with self.subTest(task=task['task_id']):
                instance=self.fixture_task(task['task_id'],'all'+task['task_id'])
                with self.g.tx() as db:db.execute('UPDATE players SET normal_ap=24')
                for attempt in range(task['target_count']):
                    identity=task['task_id']+'-'+str(attempt)
                    self.cmd('activate',{'instance_id':instance},'activate-'+identity)
                    battle=self.cmd('battle_start',key='start-'+identity)['battle_id'];facts=self.facts();facts['deck']['cards']=[identity+'c'+str(i) for i in range(8)];facts['stats']['towers']=1
                    for key,value in json.loads(task['condition_json']).items():
                        if key.endswith('_min'):
                            base=key[:-4];facts['stats' if base.endswith('_plays') else 'deck'][base]=value
                        elif key.endswith('_max'):facts['deck'][key[:-4]]=value
                        else:facts['deck'][key]=value
                    result=self.cmd('battle_settle',{'battle_id':battle,'facts':facts},'settle-'+identity)
                self.assertEqual(len(result['completed']),1)
                self.open_box(result['completed'][0]['box_id'])

    def ranking_fixture(self):
        self.fixture_task('111001','low');self.fixture_task('112001','middle');self.fixture_task('113001','high')
        with self.g.tx() as db:db.execute('UPDATE players SET normal_ap=3')
    def active_ids(self):return {t['instance_id'] for t in self.g.state('P001')['tasks'] if t['status']=='active'}
    def test_accept_automatically_reserves_without_spending(self):
        task=next(t for t in self.g.state('P001')['tasks'] if t['tier']==1)
        result=self.cmd('accept',{'task_id':task['task_id']})
        state=self.g.state('P001');self.assertEqual(result['status'],'active');self.assertEqual(state['player']['normal_ap'],5);self.assertEqual(state['player']['reserved_ap'],1)
    def test_activation_order_rebalances_and_persists(self):
        self.ranking_fixture();self.assertEqual(self.active_ids(),{'low','middle'})
        self.cmd('activate_auto',{'order':1});self.assertEqual(self.active_ids(),{'high'})
        reopened=Game(self.g.path,lambda:self.time);self.assertEqual(reopened.state('P001')['player']['activation_order'],1)
        with self.g.tx() as db:db.execute("UPDATE task_instances SET progress=1 WHERE instance_id='low'")
        self.cmd('activate_auto',{'order':2});self.assertEqual(self.active_ids(),{'low','middle'})
        for invalid in (-1,3,True,'1'):
            with self.assertRaises(GameError):self.cmd('activate_auto',{'order':invalid},'bad'+str(invalid))
    def test_force_one_displaces_auto_and_unforce_restores_order(self):
        self.ranking_fixture();self.assertEqual(self.active_ids(),{'low','middle'})
        self.cmd('force_activate',{'instance_id':'high'});self.assertEqual(self.active_ids(),{'high'})
        self.cmd('activate_auto',{'order':0});self.assertEqual(self.active_ids(),{'high'})
        self.cmd('force_activate',{'instance_id':'middle'});self.assertEqual(self.active_ids(),{'low','middle'})
        state=Game(self.g.path,lambda:self.time).state('P001');self.assertEqual([t['instance_id'] for t in state['tasks'] if t.get('forced')],['middle'])
        self.cmd('force_activate',{'instance_id':'high'},'force-again');self.cmd('unforce',{'instance_id':'high'});self.assertEqual(self.active_ids(),{'low','middle'})
        self.assertFalse(any(t.get('forced') for t in self.g.state('P001')['tasks']))
    def test_force_respects_real_ap_queue_and_ownership(self):
        self.ranking_fixture()
        with self.g.tx() as db:db.execute('UPDATE players SET normal_ap=2')
        with self.assertRaises(GameError):self.cmd('force_activate',{'instance_id':'high'})
        self.assertEqual(self.g.state('P001')['player']['forced_instance_id'],'')
        self.boxes(20)
        with self.assertRaises(GameError):self.cmd('force_activate',{'instance_id':'low'})
        self.assertEqual(self.active_ids(),set())
        with self.assertRaises(GameError):self.cmd('force_activate',{'instance_id':'someone-elses-task'})
    def test_running_battle_keeps_locked_tasks_then_replans(self):
        self.ranking_fixture();self.assertEqual(self.active_ids(),{'low','middle'})
        battle=self.cmd('battle_start')['battle_id'];self.cmd('activate_auto',{'order':1})
        self.assertEqual(self.active_ids(),{'low','middle'})
        with self.assertRaises(GameError):self.cmd('force_activate',{'instance_id':'high'})
        self.cmd('battle_settle',{'battle_id':battle,'facts':self.facts(disconnect=True)})
        self.assertEqual(self.active_ids(),{'high'})
    def test_recovery_and_opening_box_automatically_fill_capacity(self):
        self.fixture_task('111001','low');self.boxes(20)
        with self.g.tx() as db:db.execute('UPDATE players SET normal_ap=0')
        self.assertEqual(self.active_ids(),set())
        self.time+=6*3600;self.assertEqual(self.active_ids(),set())
        self.open_box('SEED0');self.assertEqual(self.active_ids(),{'low'})
    def test_automatic_reads_keep_reservation_identity(self):
        self.ranking_fixture();self.active_ids()
        with self.g.tx() as db:before=[tuple(r) for r in db.execute('SELECT activation_id,state FROM activations ORDER BY activation_id')]
        for _ in range(3):self.g.state('P001')
        with self.g.tx() as db:after=[tuple(r) for r in db.execute('SELECT activation_id,state FROM activations ORDER BY activation_id')]
        self.assertEqual(before,after)
    def test_completed_or_cancelled_forced_task_clears_override(self):
        self.fixture_task();self.settle();self.assertEqual(self.g.state('P001')['player']['forced_instance_id'],'')
        self.fixture_task('111001','cancel-me');self.cmd('force_activate',{'instance_id':'cancel-me'});self.cmd('cancel',{'instance_id':'cancel-me'})
        self.assertEqual(self.g.state('P001')['player']['forced_instance_id'],'')

if __name__=='__main__':unittest.main(verbosity=2)
