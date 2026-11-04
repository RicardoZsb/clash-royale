import unittest
from test_game import Tests
from game import GameError


class TrainingTests(unittest.TestCase):
    setUp = Tests.setUp
    tearDown = Tests.tearDown
    cmd = Tests.cmd
    fixture_task = Tests.fixture_task
    # Reuse the isolated SQLite fixture; these cover the new local battle contract.
    def training(self, outcome='win'):
        battle = self.cmd('training_start', key='training-start')['battle_id']
        result = self.cmd('training_finish', {'battle_id': battle, 'outcome': outcome,
                                            'elapsed_seconds': 180}, 'training-finish')
        return battle, result

    def test_training_completes_only_two_locked_tasks(self):
        for i in range(4):
            self.fixture_task('11100'+str(i+1), 'task'+str(i))
        battle = self.cmd('training_start', key='training-start')['battle_id']
        self.fixture_task('111005', 'accepted-after-start')
        result = self.cmd('training_finish', {'battle_id': battle, 'outcome': 'loss',
                                            'elapsed_seconds': 180}, 'training-finish')
        self.assertEqual(result['completed_count'], 2)
        self.assertEqual(result['outcome'], 'loss')
        self.assertEqual(len(result['progress']), 2)
        self.assertNotIn('accepted-after-start', {t['instance_id'] for t in result['completed']})
        before = self.g.state('P001')['player']
        replay = self.cmd('training_finish', {'battle_id': battle, 'outcome': 'win',
                                             'elapsed_seconds': 240}, 'another-key')
        self.assertEqual(replay, result)
        self.assertEqual(before, self.g.state('P001')['player'])
        self.assertEqual(before['pending_chests'], 2)

    def test_training_without_active_tasks(self):
        _, result = self.training('draw')
        self.assertEqual(result['completed_count'], 0)
        self.assertTrue(result['valid'])

    def test_training_one_active_task(self):
        self.fixture_task('111001', 'one')
        _, result = self.training()
        self.assertEqual(result['completed_count'], 1)

    def test_accepted_but_inactive_tasks_never_complete(self):
        for i in range(4):self.fixture_task('11100'+str(i+1),'limited'+str(i))
        with self.g.tx() as db:db.execute('UPDATE players SET normal_ap=2')
        state=self.g.state('P001');active={t['instance_id'] for t in state['tasks'] if t['status']=='active'}
        inactive={t['instance_id'] for t in state['tasks'] if t['status']=='accepted'}
        self.assertEqual(len(active),2);self.assertEqual(len(inactive),2)
        battle,result=self.training('loss')
        completed={t['instance_id'] for t in result['completed']}
        self.assertEqual(completed,active);self.assertFalse(completed&inactive)
        with self.g.tx() as db:
            boxes=[dict(r) for r in db.execute('SELECT box_id,source_settlement_id FROM chests')]
            saved=db.execute('SELECT completed_instances FROM training_sessions WHERE battle_id=?',(battle,)).fetchone()[0]
        self.assertEqual(len(boxes),2);self.assertEqual(len({b['source_settlement_id'] for b in boxes}),2)
        import json
        self.assertEqual(set(json.loads(saved)),active)

    def test_training_invalid_finish_preserves_reservation(self):
        self.fixture_task('111001', 'one')
        battle = self.cmd('training_start', key='training-start')['battle_id']
        with self.assertRaises(GameError):
            self.cmd('training_finish', {'battle_id': battle, 'outcome': 'invalid'}, 'bad')
        self.assertEqual(self.g.state('P001')['player']['active_count'], 1)
        with self.assertRaises(GameError):
            self.cmd('training_finish', {'battle_id': 'missing', 'outcome': 'win'}, 'missing')

    def test_restart_releases_unfinished_match_without_rewards(self):
        self.fixture_task('111001','one');self.fixture_task('111002','two')
        battle=self.cmd('training_start',key='old')['battle_id'];before=self.g.state('P001')['player']
        self.cmd('training_leave',key='return-home');after=self.g.state('P001')['player']
        for field in ['normal_ap','gold','cards','pass_xp','pending_chests']:self.assertEqual(before[field],after[field])
        self.assertEqual(after['accepted_count'],2);self.assertEqual(after['active_count'],2)
        with self.g.tx() as db:
            self.assertEqual(db.execute('SELECT outcome FROM training_sessions WHERE battle_id=?',(battle,)).fetchone()[0],'abandoned')
        result=self.cmd('training_finish',{'battle_id':battle,'outcome':'win','elapsed_seconds':180},'late-result')
        self.assertTrue(result['abandoned']);self.assertEqual(result['completed_count'],0)

    def test_fresh_match_does_not_reuse_old_empty_activation_list(self):
        old=self.cmd('training_start',key='empty')['battle_id']
        for i in range(3):self.fixture_task('11100'+str(i+1),'new'+str(i))
        self.assertEqual(self.g.state('P001')['player']['active_count'],0)
        new=self.cmd('training_start',key='fresh')['battle_id'];self.assertNotEqual(old,new)
        state=self.g.state('P001');self.assertEqual(state['player']['accepted_count'],3);self.assertEqual(state['player']['active_count'],3)
        result=self.cmd('training_finish',{'battle_id':new,'outcome':'win','elapsed_seconds':180},'finish-fresh')
        self.assertEqual(result['completed_count'],2)
        after=self.g.state('P001')['player'];self.assertEqual(after['accepted_count'],1);self.assertEqual(after['active_count'],1);self.assertEqual(after['pending_chests'],2)


if __name__ == '__main__':
    unittest.main(verbosity=2)
