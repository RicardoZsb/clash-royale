import sqlite3,json,urllib.request
from pathlib import Path
root=Path(__file__).resolve().parents[3];data=root/'Backend/.local'
auth=json.loads((data/'credentials.json').read_text(encoding='utf-8'))
req=urllib.request.Request('http://127.0.0.1:18765/v1/state',headers={'Authorization':'Bearer '+auth['client_token']})
s=json.load(urllib.request.urlopen(req));p=s['player']
print('api', {k:p.get(k) for k in ['player_id','normal_ap','accepted_count','active_count','pending_chests','premium','pass_level']})
print('tasks', {v:sum(t['status']==v for t in s['tasks']) for v in ['accepted','active','available']})
with sqlite3.connect(data/'clashroyale.sqlite3') as db:
 print('db_counts',db.execute('SELECT status,COUNT(*) FROM task_instances GROUP BY status').fetchall())
 print('running',db.execute("SELECT b.battle_id,b.started_at,COUNT(a.activation_id) FROM battles b LEFT JOIN activations a ON a.battle_id=b.battle_id AND a.state='battle' WHERE b.status='running' GROUP BY b.battle_id").fetchall())
(root/'DesignReference/Validation/TaskCounts/live-state.json').write_text(json.dumps(s,ensure_ascii=False,indent=2),encoding='utf-8')
