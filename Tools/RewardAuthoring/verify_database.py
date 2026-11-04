import sqlite3,json,urllib.request,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'Backend'))
from import_config import from_xlsx
data=ROOT/'Backend/.local';db=sqlite3.connect(data/'clashroyale.sqlite3');db.row_factory=sqlite3.Row
bundle=db.execute("SELECT value FROM service_meta WHERE key='active_bundle'").fetchone()[0]
print('active_bundle',bundle)
print('tables',[r[0] for r in db.execute("SELECT name FROM sqlite_master WHERE type='table' AND name IN ('chest_openings','player_items','pass_claims','resource_ledger')")])
print('player',dict(db.execute('SELECT player_id,premium,pass_xp,gold,gems FROM players WHERE player_id=?',('P001',)).fetchone()))
print('claims',db.execute('SELECT count(*) FROM pass_claims').fetchone()[0])
print('inventory',[dict(r) for r in db.execute('SELECT resource,amount FROM player_items')])
auth=json.loads((data/'credentials.json').read_text(encoding='utf-8'))
request=urllib.request.Request('http://127.0.0.1:18765/v1/state',headers={'Authorization':'Bearer '+auth['client_token']})
state=json.load(urllib.request.urlopen(request));config=from_xlsx(ROOT/'Assets/Tables/ClashRoyale_V5.4.xlsx')
assert state['bundle_id']==bundle
assert state['pass_levels']==config['pass_levels']
assert state['rewards']==config['rewards']
assert all(r['roll_count']<4 for r in state['chests'])
print('live_api_equals_excel',True,'pass_levels',len(state['pass_levels']),'pending_chests',len(state['chests']))
print('recent_training',[dict(r) for r in db.execute('SELECT battle_id,outcome,completed_instances FROM training_sessions ORDER BY rowid DESC LIMIT 3')])
report={'active_bundle':bundle,'live_api_equals_excel':True,'task_count':len(config['tasks']),'pass_levels':len(config['pass_levels']),'database_integrity':db.execute('PRAGMA integrity_check').fetchone()[0],'foreign_key_errors':[list(r) for r in db.execute('PRAGMA foreign_key_check')]}
(ROOT/'DesignReference/Validation/Rewards/database-validation.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
print('integrity',report['database_integrity'],'foreign_key_errors',len(report['foreign_key_errors']))
