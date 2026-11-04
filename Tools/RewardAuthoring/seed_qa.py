import sys,sqlite3,json
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'Backend'))
from game import Game
from import_config import from_xlsx,read_tables
from server import credentials
data=ROOT/'DesignReference/Validation/Rewards/.local';data.mkdir(exist_ok=True)
game=Game(data/'clashroyale.sqlite3');game.install(from_xlsx(ROOT/'Assets/Tables/ClashRoyale_V5.4.xlsx'),'QA workbook');game.create_player();credentials(data)
with game.tx() as db:
    c=game.config(db);db.execute('UPDATE players SET pass_xp=200,normal_ap=16,premium=0')
    for star in range(1,6):db.execute('INSERT OR IGNORE INTO chests(box_id,player_id,bundle_id,chest_id,initial_star,created_at) VALUES(?,?,?,?,?,?)',('QA'+str(star),'P001',c['_bundle'],'600001',star,game.now()+star))
original=json.loads((ROOT/'Tools/RewardAuthoring/original-tables.json').read_text(encoding='utf-8'));current=read_tables(ROOT/'Assets/Tables/ClashRoyale_V5.4.xlsx')
for name,rows in original.items():
    if name not in ('奖励','战令40级','行动点与规则'):assert rows==current[name],name
print('QA ready; unrelated workbook sheets unchanged.')
