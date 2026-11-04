import sys
from pathlib import Path
from http.server import ThreadingHTTPServer

root=Path(__file__).resolve().parents[3]
sys.path.insert(0,str(root/'Backend'))
from game import Game
from import_config import from_xlsx
from server import credentials,handler

directory=root/'Backend/.local/pass-claim-qa'
directory.mkdir(parents=True,exist_ok=True)
book=root/'Assets/Tables/ClashRoyale_V5.4.xlsx'
game=Game(directory/'validation.sqlite3')
game.install(from_xlsx(book),'single-claim-validation');game.create_player()
with game.tx() as db:
    db.execute('UPDATE players SET pass_xp=14,premium=1')
game.command('P001','preview-first-free','claim_pass',{'level':1,'track':'FREE'})
game.command('P001','preview-first-premium','claim_pass',{'level':1,'track':'PREMIUM'})
auth=credentials(directory)
ThreadingHTTPServer(('127.0.0.1',18766),handler(game,auth,book)).serve_forever()
