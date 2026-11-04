"""Loopback-only development service. Unity's token cannot submit battle results."""
import argparse,hashlib,json,os,secrets,sys,traceback
from http.server import BaseHTTPRequestHandler,ThreadingHTTPServer
from pathlib import Path
from game import Game,GameError,dumps
from import_config import from_xlsx

ROOT=Path(__file__).resolve().parent
DEFAULT_BOOK=ROOT.parent/'Assets/Tables/ClashRoyale_V5.4.xlsx'

def credentials(directory):
    path=directory/'credentials.json'
    if path.exists():return json.loads(path.read_text(encoding='utf-8'))
    data={'client_token':secrets.token_urlsafe(32),'admin_token':secrets.token_urlsafe(40)}
    path.write_text(json.dumps(data),encoding='utf-8');return data

def handler(game,auth,book):
    class Handler(BaseHTTPRequestHandler):
        server_version='ClashRoyaleLocal/1.0'
        def log_message(self,fmt,*args):
            print('%s %s'%(self.log_date_time_string(),fmt%args),flush=True)
        def reply(self,status,data):
            raw=dumps(data).encode('utf-8');self.send_response(status);self.send_header('Content-Type','application/json; charset=utf-8');self.send_header('Content-Length',str(len(raw)));self.send_header('Cache-Control','no-store');self.end_headers();self.wfile.write(raw)
        def authorized(self,admin=False):
            got=self.headers.get('Authorization','');want='Bearer '+auth['admin_token' if admin else 'client_token']
            if not secrets.compare_digest(got,want):raise GameError('无效访问凭证',401)
        def do_GET(self):
            try:
                if self.path=='/health':return self.reply(200,{'ok':True,'service':'ClashRoyaleV54','schema':1})
                self.authorized()
                if self.path=='/v1/state':return self.reply(200,game.state('P001'))
                raise GameError('接口不存在',404)
            except GameError as e:self.reply(e.code,{'ok':False,'error':str(e)})
            except Exception:traceback.print_exc();self.reply(500,{'ok':False,'error':'服务器读取失败，请查看本机日志'})
        def do_POST(self):
            try:
                admin=self.path.startswith('/admin/');self.authorized(admin)
                length=int(self.headers.get('Content-Length','0'))
                if length<0 or length>65536:raise GameError('请求过大',413)
                body=json.loads(self.rfile.read(length).decode('utf-8') or '{}')
                if not isinstance(body,dict):raise GameError('请求必须为对象',400)
                path=self.path.strip('/').split('/');data=body.copy();op=None
                if self.path=='/admin/config/import':
                    bundle=game.install(from_xlsx(book),book);return self.reply(200,{'ok':True,'bundle_id':bundle})
                if admin and len(path)==4 and path[1]=='battles' and path[3]=='settle':op='battle_settle';data['battle_id']=path[2]
                elif path[:2]==['v1','tasks'] and len(path)==4 and path[3]=='accept':op='accept';data['task_id']=path[2]
                elif path[:2]==['v1','instances'] and len(path)==4 and path[3] in ('activate','force-activate','unforce','deactivate','cancel','help'):op=path[3].replace('-','_');data['instance_id']=path[2]
                elif self.path=='/v1/tasks/activate-auto':op='activate_auto'
                elif self.path=='/v1/battles/start':op='battle_start'
                elif self.path=='/v1/training/start':op='training_start'
                elif self.path=='/v1/training/leave':op='training_leave'
                elif len(path)==4 and path[:2]==['v1','training'] and path[3]=='finish':op='training_finish';data['battle_id']=path[2]
                elif self.path=='/v1/reserve/withdraw':op='withdraw'
                elif self.path=='/v1/pass/unlock':op='unlock_pass'
                elif self.path=='/v1/shop/action-point':op='buy_ap'
                elif path[:2]==['v1','chests'] and len(path)==4 and path[3]=='open':op='open_chest';data['box_id']=path[2]
                elif path[:2]==['v1','pass'] and len(path)==4 and path[3]=='claim':op='claim_pass';data['level']=int(path[2])
                if not op:raise GameError('接口不存在',404)
                result=game.command('P001',self.headers.get('Idempotency-Key',''),op,data)
                self.reply(200,{'ok':True,'result':result})
            except (ValueError,KeyError,TypeError) as e:self.reply(400,{'ok':False,'error':'请求或配置格式错误：'+str(e)})
            except GameError as e:self.reply(e.code,{'ok':False,'error':str(e)})
            except Exception:traceback.print_exc();self.reply(500,{'ok':False,'error':'事务已回滚，请重试或查看本机日志'})
    return Handler

if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('--port',type=int,default=18765);parser.add_argument('--book',type=Path,default=DEFAULT_BOOK);parser.add_argument('--data',type=Path,default=ROOT/'.local');args=parser.parse_args()
    args.data.mkdir(exist_ok=True);game=Game(args.data/'clashroyale.sqlite3');game.install(from_xlsx(args.book),args.book);game.create_player()
    auth=credentials(args.data);server=ThreadingHTTPServer(('127.0.0.1',args.port),handler(game,auth,args.book))
    print('Clash Royale V5.4 local backend http://127.0.0.1:%s | 62 task templates loaded'%args.port,flush=True)
    server.serve_forever()
