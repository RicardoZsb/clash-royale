import json
import sqlite3
import urllib.request
import urllib.error
from pathlib import Path

auth = json.loads(Path('Backend/.local/credentials.json').read_text())
req = urllib.request.Request('http://127.0.0.1:18765/v1/training/check-missing/finish',
    data=json.dumps({'outcome': 'win'}).encode(), headers={
        'Authorization': 'Bearer '+auth['client_token'], 'Content-Type': 'application/json',
        'Idempotency-Key': 'verify-training-route'})
try:
    urllib.request.urlopen(req)
except urllib.error.HTTPError as e:
    print(e.code, e.read().decode())
queries = ['select normal_ap,gold,pass_xp from players',
           'select count(*) from settlements', 'select count(*) from chests']
with sqlite3.connect('DesignReference/Backups/TrainingBattle-20261102/clashroyale.sqlite3') as old:
    with sqlite3.connect('Backend/.local/clashroyale.sqlite3') as new:
        print('Player data unchanged:', all(old.execute(q).fetchall() == new.execute(q).fetchall() for q in queries))
