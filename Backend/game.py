"""Server-owned V5.4 state. All commands execute inside one SQLite write transaction."""
import contextlib, datetime as dt, hashlib, json, random, secrets, sqlite3, time, uuid
from pathlib import Path

ROOT=Path(__file__).resolve().parent
def dumps(x): return json.dumps(x,ensure_ascii=False,separators=(',',':'),sort_keys=True)
def uid(prefix): return prefix+uuid.uuid4().hex
class GameError(Exception):
    def __init__(self,message,code=409): self.code=code; super().__init__(message)

class Game:
    def __init__(self,path,clock=time.time,rng=None):
        self.path=str(path); self.clock=clock; self.rng=rng or random.SystemRandom()
        Path(path).parent.mkdir(parents=True,exist_ok=True)
        with contextlib.closing(self.connect()) as db: db.executescript((ROOT/'schema.sql').read_text(encoding='utf-8'))
    def connect(self):
        db=sqlite3.connect(self.path,timeout=15,isolation_level=None)
        db.row_factory=sqlite3.Row; db.execute('PRAGMA foreign_keys=ON'); db.execute('PRAGMA journal_mode=WAL'); db.execute('PRAGMA busy_timeout=15000')
        return db
    @contextlib.contextmanager
    def tx(self):
        db=self.connect()
        try:
            db.execute('BEGIN IMMEDIATE'); yield db; db.commit()
        except Exception: db.rollback(); raise
        finally: db.close()
    def now(self): return int(self.clock())
    def period(self):
        date=dt.datetime.fromtimestamp(self.now(),dt.timezone(dt.timedelta(hours=8)))
        return date.strftime('%G-W%V'),date.strftime('%Y-%m-%d')
    def config(self,db,bundle=None):
        if not bundle:
            row=db.execute("SELECT value FROM service_meta WHERE key='active_bundle'").fetchone()
            if not row: raise GameError('尚未导入 Excel 配置',503)
            bundle=row[0]
        row=db.execute('SELECT payload FROM config_bundles WHERE bundle_id=?',(bundle,)).fetchone()
        if not row: raise GameError('配置版本不存在',404)
        c=json.loads(row[0]); c['_bundle']=bundle; c['_rules']={r['key']:r['value'] for r in c['rules']}; return c
    def install(self,config,source):
        from import_config import validate
        validate(config); payload=dumps(config); sha=hashlib.sha256(payload.encode()).hexdigest(); bundle=config['version']+'-'+sha[:10]
        with self.tx() as db:
            db.execute('INSERT OR IGNORE INTO config_bundles VALUES(?,?,?,?,?)',(bundle,sha,self.now(),str(source),payload))
            for t in config['tasks']:
                db.execute('INSERT OR IGNORE INTO task_templates VALUES(?,?,?,?,?,?,?)',(bundle,t['task_id'],t['tier'],t['title'],t['brief'],t['description'],dumps(t)))
            db.execute("INSERT OR REPLACE INTO service_meta VALUES('active_bundle',?)",(bundle,))
        return bundle
    def create_player(self,player='P001',case=False):
        with self.tx() as db:
            c=self.config(db)
            db.execute('INSERT OR IGNORE INTO players(player_id,season_id,normal_ap,reserve_ap,gold,cards,gems,pass_xp,tokens,season_contribution,clan_id,joined_at,recovery_at) VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?)',
                (player,c['season_id'],5 if case else 16,8 if case else 16,1000 if case else 16545,10,16545,3 if case else 0,5 if case else 0,15 if case else 0,'C001',self.now()-4*86400,self.now()))
            db.execute('INSERT OR IGNORE INTO clan_members VALUES(?,?,?,?,?,?)',(player,'C001',c['season_id'],15 if case else 0,1 if case else 0,1))
            if case:
                for i in range(30): db.execute('INSERT OR IGNORE INTO clan_members VALUES(?,?,?,?,?,?)',(f'CASE-{i}','C001',c['season_id'],50,1,1))
    def player(self,db,p):
        row=db.execute('SELECT * FROM players WHERE player_id=?',(p,)).fetchone()
        if not row: raise GameError('玩家不存在',404)
        return dict(row)
    def event(self,db,p,kind,data): db.execute('INSERT INTO events(player_id,kind,payload,created_at) VALUES(?,?,?,?)',(p,kind,dumps(data),self.now()))
    def change(self,db,p,resource,amount,kind,source):
        allowed={'normal_ap','reserve_ap','gold','gems','cards','tokens','pass_xp'}
        if resource not in allowed: raise ValueError(resource)
        db.execute(f'UPDATE players SET {resource}={resource}+? WHERE player_id=?',(amount,p))
        balance=db.execute(f'SELECT {resource} FROM players WHERE player_id=?',(p,)).fetchone()[0]
        db.execute('INSERT INTO resource_ledger(player_id,resource,amount,source_type,source_id,balance,created_at) VALUES(?,?,?,?,?,?,?)',(p,resource,amount,kind,source,balance,self.now()))
    def recover(self,db,p,c):
        a=self.player(db,p); rules=c['_rules']; interval=int(rules['recovery_hours']*3600); ticks=max(0,(self.now()-a['recovery_at'])//interval)
        if not ticks:return
        normal=a['normal_ap']; reserve=a['reserve_ap']; cap=rules['normal_cap']; rcap=rules['reserve_cap']
        # Bound work even for long offline periods; remaining full ticks simply advance the clock.
        for _ in range(min(ticks,cap+rcap)):
            if normal<cap: normal=min(cap,normal+rules['normal_recovery'])
            else: reserve=min(rcap,reserve+rules['reserve_recovery'])
        source=str(a['recovery_at'])
        if normal!=a['normal_ap']:self.change(db,p,'normal_ap',normal-a['normal_ap'],'recovery',source)
        if reserve!=a['reserve_ap']:self.change(db,p,'reserve_ap',reserve-a['reserve_ap'],'recovery',source)
        db.execute('UPDATE players SET recovery_at=? WHERE player_id=?',(a['recovery_at']+ticks*interval,p))
    def reservations(self,db,p):
        r=db.execute("SELECT COALESCE(SUM(ap_reserved),0),COALESCE(SUM(slots_reserved),0) FROM activations WHERE player_id=? AND state IN ('reserved','battle')",(p,)).fetchone()
        return r[0],r[1]
    def expire(self,db,p):
        week,_=self.period()
        rows=db.execute("SELECT instance_id FROM task_instances WHERE player_id=? AND week_id<>? AND status IN ('accepted','active') AND instance_id NOT IN (SELECT instance_id FROM activations WHERE state='battle')",(p,week)).fetchall()
        for row in rows:
            db.execute("UPDATE activations SET state='released' WHERE instance_id=? AND state='reserved'",(row[0],))
            db.execute("UPDATE task_instances SET status='expired' WHERE instance_id=?",(row[0],))
    def eligible(self,t,a):
        return t['enabled'] and a['audience'] in t['audience'].split(',') and (a['clan_id'] or not json.loads(t['condition_json']).get('clan_shared'))
    def displayed(self,db,p,c):
        week,day=self.period(); a=self.player(db,p)
        done={r[0] for r in db.execute("SELECT task_id FROM task_instances WHERE player_id=? AND week_id=? AND status IN ('completed','accepted','active')",(p,week))}
        result=[]
        for quota in c['pool']:
            ts=[t for t in c['tasks'] if t['tier']==quota['tier'] and self.eligible(t,a)]
            ts.sort(key=lambda t:hashlib.sha256((p+week+day+t['task_id']).encode()).hexdigest())
            result.extend(t for t in ts[:quota['daily_count']] if t['task_id'] not in done)
        return result
    def reward(self,db,p,c,reward_id,kind,source):
        reward=next((x for x in c['rewards'] if x['reward_id']==reward_id),None)
        if reward is None: raise GameError('奖励配置缺失',500)
        a=self.player(db,p); ap=reward['ap']
        if ap and (a['normal_ap']+ap>c['_rules']['normal_cap']):raise GameError('行动点空间不足，请先消耗后领取')
        if ap and a['extra_ap_used']+ap>c['_rules']['extra_ap_season_budget']:raise GameError('本赛季额外行动点额度已用完')
        for field in ['gold','cards','ap']:
            if reward[field]:self.change(db,p,'normal_ap' if field=='ap' else field,reward[field],kind,source)
        resource=reward.get('resource');amount=reward.get('amount',0)
        if resource and amount:
            if resource in ('gems','gold','cards'):self.change(db,p,resource,amount,kind,source)
            else:
                db.execute('INSERT INTO player_items VALUES(?,?,?) ON CONFLICT(player_id,resource) DO UPDATE SET amount=amount+excluded.amount',(p,resource,amount))
                balance=db.execute('SELECT amount FROM player_items WHERE player_id=? AND resource=?',(p,resource)).fetchone()[0]
                db.execute('INSERT INTO resource_ledger(player_id,resource,amount,source_type,source_id,balance,created_at) VALUES(?,?,?,?,?,?,?)',(p,resource,amount,kind,source,balance,self.now()))
        if reward.get('chest_star',0):
            pending=db.execute("SELECT COUNT(*) FROM chests WHERE player_id=? AND status='pending'",(p,)).fetchone()[0]
            if pending+self.reservations(db,p)[1]>=c['_rules']['queue_capacity']:raise GameError('宝箱队列已满')
            db.execute('INSERT INTO chests(box_id,player_id,bundle_id,chest_id,initial_star,created_at) VALUES(?,?,?,?,?,?)',(uid('BOX'),p,c['_bundle'],'PASS',reward['chest_star'],self.now()))
        if ap:db.execute('UPDATE players SET extra_ap_used=extra_ap_used+? WHERE player_id=?',(ap,p))
        return reward
    def get_instance(self,db,p,i):
        row=db.execute('SELECT * FROM task_instances WHERE instance_id=? AND player_id=?',(i,p)).fetchone()
        if not row: raise GameError('任务实例不存在',404)
        return dict(row)
    def command(self,p,request_id,operation,data=None):
        data=data or {}
        if not request_id or len(request_id)>128:raise GameError('缺少有效请求编号',400)
        fingerprint=hashlib.sha256(dumps([operation,data]).encode()).hexdigest()
        with self.tx() as db:
            old=db.execute('SELECT * FROM request_results WHERE player_id=? AND request_id=?',(p,request_id)).fetchone()
            if old:
                if old['request_hash']!=fingerprint:raise GameError('同一请求编号不能用于不同操作')
                return json.loads(old['result'])
            c=self.config(db);self.player(db,p);self.recover(db,p,c);self.expire(db,p)
            result=self.dispatch(db,p,c,operation,data)
            self.reconcile_activations(db,p,c)
            if 'instance_id' in result:
                instance=self.get_instance(db,p,result['instance_id'])
                result['status']=instance['status']
            db.execute('INSERT INTO request_results VALUES(?,?,?,?,?)',(p,request_id,fingerprint,dumps(result),self.now()))
            self.event(db,p,operation,result)
            return result
    def activation_preferences(self,db,p):
        db.execute('INSERT OR IGNORE INTO task_activation_preferences(player_id) VALUES(?)',(p,))
        return dict(db.execute('SELECT * FROM task_activation_preferences WHERE player_id=?',(p,)).fetchone())
    def leave_training(self,db,p):
        battles=db.execute("SELECT b.battle_id FROM battles b JOIN training_sessions t ON t.battle_id=b.battle_id WHERE b.player_id=? AND b.status='running'",(p,)).fetchall()
        for row in battles:
            battle=row['battle_id']
            db.execute("UPDATE task_instances SET status='accepted' WHERE instance_id IN (SELECT instance_id FROM activations WHERE battle_id=? AND state='battle')",(battle,))
            db.execute("UPDATE activations SET state='released' WHERE battle_id=? AND state='battle'",(battle,))
            result={'battle_id':battle,'abandoned':True,'valid':False,'completed':[],'progress':[],'completed_count':0}
            db.execute("UPDATE battles SET status='settled',settled_at=?,result=? WHERE battle_id=?",(self.now(),dumps(result),battle))
            db.execute("UPDATE training_sessions SET outcome='abandoned',completed_instances='[]' WHERE battle_id=?",(battle,))
        return {'abandoned_count':len(battles)}
    def reconcile_activations(self,db,p,c):
        pref=self.activation_preferences(db,p)
        if db.execute("SELECT 1 FROM battles WHERE player_id=? AND status='running'",(p,)).fetchone():return
        rows=[dict(r) for r in db.execute("SELECT * FROM task_instances WHERE player_id=? AND status IN ('accepted','active') ORDER BY accepted_at,rowid",(p,))]
        if pref['forced_instance_id'] and not any(r['instance_id']==pref['forced_instance_id'] for r in rows):
            db.execute('UPDATE task_activation_preferences SET forced_instance_id=NULL WHERE player_id=?',(p,));pref['forced_instance_id']=None
        for row in rows:row['_task']=json.loads(row['config_snapshot'])
        if pref['order_rule']==1:rows.sort(key=lambda r:-r['_task']['tier'])
        elif pref['order_rule']==2:rows.sort(key=lambda r:-r['progress']/r['_task']['target_count'])
        else:rows.sort(key=lambda r:r['_task']['tier'])
        rows.sort(key=lambda r:r['instance_id']!=pref['forced_instance_id'])
        remaining=self.player(db,p)['normal_ap'];slots=c['_rules']['queue_capacity']-db.execute("SELECT COUNT(*) FROM chests WHERE player_id=? AND status='pending'",(p,)).fetchone()[0]
        selected=[]
        if c['_rules']['tasks_enabled']:
            for row in rows:
                cost=row['_task']['action_cost']
                if cost<=remaining and slots>0:selected.append(row['instance_id']);remaining-=cost;slots-=1
        chosen=set(selected)
        # Preserve reservations that remain selected so reads cannot churn activation IDs.
        for act in db.execute("SELECT activation_id,instance_id FROM activations WHERE player_id=? AND state='reserved'",(p,)).fetchall():
            if act['instance_id'] not in chosen:
                db.execute("UPDATE activations SET state='released' WHERE activation_id=?",(act['activation_id'],))
                db.execute("UPDATE task_instances SET status='accepted' WHERE instance_id=?",(act['instance_id'],))
        for i in selected:self.reserve_activation(db,p,c,i)
    def force_activate(self,db,p,c,i):
        if not c['_rules']['tasks_enabled']:raise GameError('任务暂未开放')
        if db.execute("SELECT 1 FROM battles WHERE player_id=? AND status='running'",(p,)).fetchone():raise GameError('对局中不能强制激活任务，请在对局结束后调整')
        instance=self.get_instance(db,p,i)
        if instance['status'] not in ('accepted','active'):raise GameError('请先接取任务')
        cost=json.loads(instance['config_snapshot'])['action_cost']
        if self.player(db,p)['normal_ap']<cost:raise GameError('行动点不足，无法强制激活')
        if db.execute("SELECT COUNT(*) FROM chests WHERE player_id=? AND status='pending'",(p,)).fetchone()[0]>=c['_rules']['queue_capacity']:raise GameError('宝箱队列已满，请先开启宝箱')
        self.activation_preferences(db,p)
        db.execute('UPDATE task_activation_preferences SET forced_instance_id=? WHERE player_id=?',(i,p))
        return {'instance_id':i,'message':'已优先激活该任务'}
    def reserve_activation(self,db,p,c,i):
        if not c['_rules']['tasks_enabled']:raise GameError('任务暂未开放')
        if db.execute("SELECT 1 FROM battles WHERE player_id=? AND status='running'",(p,)).fetchone():raise GameError('对局中不能调整激活任务')
        a=self.get_instance(db,p,i)
        if a['status']=='active':return {'instance_id':i,'status':'active'}
        if a['status']!='accepted':raise GameError('请先接取任务')
        t=json.loads(a['config_snapshot']); balance=self.player(db,p); ap,slots=self.reservations(db,p)
        boxes=db.execute("SELECT count(*) FROM chests WHERE player_id=? AND status='pending'",(p,)).fetchone()[0]
        if balance['normal_ap']-ap<t['action_cost']:raise GameError('可用行动点不足')
        if boxes+slots>=c['_rules']['queue_capacity']:raise GameError('宝箱队列已满，请先开启宝箱')
        activation=uid('ACT');db.execute('INSERT INTO activations VALUES(?,?,?,?,?,?,?,?)',(activation,i,p,None,t['action_cost'],1,'reserved',self.now()))
        db.execute("UPDATE task_instances SET status='active' WHERE instance_id=?",(i,))
        return {'instance_id':i,'activation_id':activation,'status':'active'}
    def dispatch(self,db,p,c,op,d):
        if op=='accept':
            if not c['_rules']['tasks_enabled']:raise GameError('任务暂未开放')
            task=next((t for t in self.displayed(db,p,c) if t['task_id']==d['task_id']),None)
            if not task:
                existing=db.execute("SELECT instance_id,status FROM task_instances WHERE player_id=? AND task_id=? AND week_id=? AND status IN ('accepted','active')",(p,d['task_id'],self.period()[0])).fetchone()
                if existing:return dict(existing)
                raise GameError('该任务当前不可接取，请刷新列表')
            i=uid('A');db.execute('INSERT INTO task_instances(instance_id,player_id,task_id,bundle_id,season_id,week_id,status,accepted_at,config_snapshot) VALUES(?,?,?,?,?,?,?,?,?)',(i,p,task['task_id'],c['_bundle'],c['season_id'],self.period()[0],'accepted',self.now(),dumps(task)))
            return {'instance_id':i,'status':'accepted'}
        if op in ('activate','force_activate'):return self.force_activate(db,p,c,d['instance_id'])
        if op=='activate_auto':
            order=d.get('order',0)
            if type(order) is not int or order not in (0,1,2):raise GameError('无效激活规则',400)
            self.activation_preferences(db,p)
            db.execute('UPDATE task_activation_preferences SET order_rule=? WHERE player_id=?',(order,p))
            locked=bool(db.execute("SELECT 1 FROM battles WHERE player_id=? AND status='running'",(p,)).fetchone())
            return {'order':order,'message':'激活规则已保存，下场对局生效' if locked else '已按规则自动激活任务'}
        if op in ('deactivate','unforce','cancel'):
            i=self.get_instance(db,p,d['instance_id'])
            if i['status'] not in ('accepted','active'):raise GameError('该任务不可操作')
            if db.execute("SELECT 1 FROM activations WHERE instance_id=? AND state='battle'",(i['instance_id'],)).fetchone():raise GameError('对局中的任务已锁定')
            if op=='cancel':
                start=((self.now()+8*3600)//86400)*86400-8*3600
                count=db.execute("SELECT count(*) FROM events WHERE player_id=? AND kind='cancel' AND created_at>=?",(p,start)).fetchone()[0]
                a=self.player(db,p);fee=0 if count<c['_rules']['daily_free_cancels'] or a['audience']=='NEW' else a['tower_level']*c['_rules']['cancel_gold_per_tower']
                if fee>a['gold']:raise GameError('金币不足，无法取消任务')
                if fee:self.change(db,p,'gold',-fee,'cancel',i['instance_id'])
            self.activation_preferences(db,p)
            db.execute('UPDATE task_activation_preferences SET forced_instance_id=NULL WHERE player_id=? AND forced_instance_id=?',(p,i['instance_id']))
            db.execute("UPDATE activations SET state='released' WHERE instance_id=? AND state='reserved'",(i['instance_id'],))
            status='cancelled' if op=='cancel' else 'accepted';db.execute('UPDATE task_instances SET status=? WHERE instance_id=?',(status,i['instance_id']))
            return {'instance_id':i['instance_id'],'status':status,'message':'任务已取消' if op=='cancel' else '已取消强制，恢复自动规则'}
        if op=='help':
            i=self.get_instance(db,p,d['instance_id']);a=self.player(db,p)
            if i['status'] not in ('accepted','active'):raise GameError('接取任务后才能请求部落求助')
            if not a['clan_id']:raise GameError('请先加入部落')
            if not c['_rules']['clan_enabled']:raise GameError('部落功能暂未开放')
            old=db.execute('SELECT help_id FROM task_help_requests WHERE instance_id=?',(i['instance_id'],)).fetchone()
            if old:return {'help_id':old[0],'message':'已向部落求助'}
            help_id=uid('H');title=json.loads(i['config_snapshot'])['brief'];db.execute('INSERT INTO task_help_requests VALUES(?,?,?,?,?,?)',(help_id,i['instance_id'],p,a['clan_id'],'请求协助：'+title,self.now()))
            return {'help_id':help_id,'message':'已向部落求助'}
        if op=='withdraw':
            a=self.player(db,p); amount=min(a['reserve_ap'],max(0,c['_rules']['normal_cap']-a['normal_ap']))
            if amount:
                source=uid('W');self.change(db,p,'normal_ap',amount,'reserve_withdraw',source);self.change(db,p,'reserve_ap',-amount,'reserve_withdraw',source)
            return {'amount':amount,'message':'行动点已满' if a['normal_ap']+amount>=c['_rules']['normal_cap'] else '提取成功' if amount else '暂无结余'}
        if op=='battle_start':
            if not c['_rules']['tasks_enabled']:raise GameError('任务暂未开放')
            old=db.execute("SELECT battle_id FROM battles WHERE player_id=? AND status='running'",(p,)).fetchone()
            if old:return {'battle_id':old[0]}
            self.reconcile_activations(db,p,c)
            b=uid('B');db.execute('INSERT INTO battles(battle_id,player_id,bundle_id,season_id,week_id,status,started_at) VALUES(?,?,?,?,?,?,?)',(b,p,c['_bundle'],c['season_id'],self.period()[0],'running',self.now()))
            db.execute("UPDATE activations SET battle_id=?,state='battle' WHERE player_id=? AND state='reserved'",(b,p))
            return {'battle_id':b}
        if op=='training_start':
            self.leave_training(db,p)
            result=self.dispatch(db,p,c,'battle_start',{})
            db.execute('INSERT OR IGNORE INTO training_sessions(battle_id,player_id) VALUES(?,?)',(result['battle_id'],p))
            return result
        if op=='training_leave':return self.leave_training(db,p)
        if op=='training_finish':
            session=db.execute('SELECT * FROM training_sessions WHERE battle_id=? AND player_id=?',(d['battle_id'],p)).fetchone()
            if not session:raise GameError('训练对局不存在',404)
            old=db.execute('SELECT status,result FROM battles WHERE battle_id=?',(d['battle_id'],)).fetchone()
            if old['status']=='settled':return json.loads(old['result'])
            outcome=d.get('outcome');elapsed=d.get('elapsed_seconds',0)
            if outcome not in ('win','loss','draw') or type(elapsed) is not int or elapsed<0:raise GameError('无效对局结果',400)
            locked=[r[0] for r in db.execute("SELECT instance_id FROM activations WHERE battle_id=? AND player_id=? AND state='battle' ORDER BY instance_id",(d['battle_id'],p))]
            selected=set(sorted(locked,key=lambda _:self.rng.random())[:2])
            facts={'mode':'training','won':outcome=='win','deck':{'cards':['knight','archer','giant','dragon','witch','fireball','arrows','zap']},'stats':{}}
            result=self.settle(db,p,c,{'battle_id':d['battle_id'],'facts':facts},selected)
            result.update(outcome=outcome,completed_count=len(result['completed']))
            db.execute('UPDATE battles SET result=? WHERE battle_id=?',(dumps(result),d['battle_id']))
            db.execute('UPDATE training_sessions SET outcome=?,completed_instances=?,elapsed_seconds=? WHERE battle_id=?',(outcome,dumps(sorted(selected)),elapsed,d['battle_id']))
            return result
        if op=='battle_settle':return self.settle(db,p,c,d)
        if op=='open_chest':
            row=db.execute('SELECT * FROM chests WHERE box_id=? AND player_id=?',(d['box_id'],p)).fetchone()
            if not row:raise GameError('宝箱不存在',404)
            cfg=self.config(db,row['bundle_id'])
            if row['status']=='opened':
                reward=next(x for x in cfg['rewards'] if x['reward_id']==row['reward_id'])
                return {'box_id':row['box_id'],'final_star':row['final_star'],'roll_count':4,'reward_id':row['reward_id'],'reward':reward,'opened':True}
            db.execute('INSERT OR IGNORE INTO chest_openings(box_id,current_star) VALUES(?,?)',(row['box_id'],row['initial_star']))
            opening=db.execute('SELECT * FROM chest_openings WHERE box_id=?',(row['box_id'],)).fetchone()
            step=int(d.get('step',0));count=opening['roll_count'];rolls=json.loads(opening['rolls']);star=opening['current_star']
            if not 1<=step<=4 or step>count+1:raise GameError('开箱步骤无效',400)
            upgraded=False
            if step==count+1:
                success=self.rng.random()<cfg['_rules']['upgrade_probability']
                upgraded=success and star<5;star=min(5,star+int(success));rolls.append(upgraded);count+=1
                db.execute('UPDATE chest_openings SET current_star=?,roll_count=?,rolls=? WHERE box_id=?',(star,count,dumps(rolls),row['box_id']))
            result={'box_id':row['box_id'],'final_star':star,'roll_count':count,'upgraded':upgraded,'opened':count==4}
            if count==4:
                reward_id=next(x['reward_id'] for x in cfg['stars'] if x['star']==star)
                result['reward']=self.reward(db,p,cfg,reward_id,'chest',row['box_id']);result['reward_id']=reward_id
                db.execute("UPDATE chests SET status='opened',final_star=?,rolls=?,reward_id=?,opened_at=? WHERE box_id=?",(star,dumps(rolls),reward_id,self.now(),row['box_id']))
            return result
        if op=='unlock_pass':
            db.execute('UPDATE players SET premium=1 WHERE player_id=?',(p,))
            return {'premium':1}
        if op=='claim_pass':
            if not c['_rules']['pass_enabled']:raise GameError('战令暂未开放')
            a=self.player(db,p);level=int(d['level']);track=d.get('track','FREE')
            if track not in ('FREE','PREMIUM'):raise GameError('无效奖励轨道',400)
            config=next((x for x in c['pass_levels'] if x['level']==level),None)
            if not config:raise GameError('战令等级不存在',404)
            if a['pass_xp']<config['required_xp']:raise GameError('战令经验不足')
            if track=='PREMIUM' and not a['premium']:raise GameError('付费战令尚未开通')
            if db.execute('SELECT 1 FROM pass_claims WHERE player_id=? AND season_id=? AND level=? AND track=?',(p,a['season_id'],level,track)).fetchone():return {'level':level,'track':track,'already_claimed':True}
            reward=config['free_reward_id'] if track=='FREE' else config['premium_reward_id']
            granted=self.reward(db,p,c,reward,'pass',a['season_id']+':'+str(level)+':'+track)
            db.execute('INSERT INTO pass_claims VALUES(?,?,?,?,?,?)',(p,a['season_id'],level,track,reward,self.now()))
            return {'level':level,'track':track,'reward_id':reward,'reward':granted}
        if op=='buy_ap':
            a=self.player(db,p);r=c['_rules']
            if not a['clan_id'] or self.now()-a['joined_at']<r['clan_cooldown_days']*86400:raise GameError('当前不满足部落商店条件')
            if a['normal_ap']>=r['normal_cap']:raise GameError('行动点已满')
            if a['tokens']<r['shop_ap_price']:raise GameError('部落代币不足')
            if a['shop_ap_bought']>=r['shop_ap_season_limit'] or a['extra_ap_used']>=r['extra_ap_season_budget']:raise GameError('本赛季购买额度已用完')
            source=uid('SHOP');self.change(db,p,'tokens',-r['shop_ap_price'],'shop',source);self.change(db,p,'normal_ap',1,'shop',source)
            db.execute('UPDATE players SET shop_ap_bought=shop_ap_bought+1,extra_ap_used=extra_ap_used+1 WHERE player_id=?',(p,));return {'ap':1}
        raise GameError('未知操作',404)
    def condition_passes(self,task,facts):
        values=facts.get('deck',{});stats=facts.get('stats',{})
        for key,want in json.loads(task['condition_json']).items():
            if key.endswith('_min'):
                base=key[:-4];actual=stats.get(base,0) if base.endswith('_plays') else values.get(base,0)
                if actual<want:return False
            elif key.endswith('_max'):
                if values.get(key[:-4],999)>want:return False
            elif values.get(key)!=want:return False
        return True
    def settle(self,db,p,current,d,training_ids=None):
        b=db.execute('SELECT * FROM battles WHERE battle_id=? AND player_id=?',(d['battle_id'],p)).fetchone()
        if not b:raise GameError('对局不存在',404)
        if b['status']=='settled':return json.loads(b['result'])
        facts=d['facts'];self.validate_facts(facts)
        if b['season_id']!=self.player(db,p)['season_id']:raise GameError('跨赛季记账规则待确认，保留对局等待结算')
        valid=training_ids is not None or facts['mode'] in ('ladder','ranked','challenge') and not any(facts.get(k,False) for k in ('surrender','disconnect','afk','passive','repeated_opponent'))
        completed=[]; progress=[]
        rows=db.execute("SELECT * FROM activations WHERE battle_id=? AND state='battle' ORDER BY created_at,activation_id",(b['battle_id'],)).fetchall()
        for act in rows:
            inst=self.get_instance(db,p,act['instance_id']);task=json.loads(inst['config_snapshot']);cfg=self.config(db,inst['bundle_id']);new=inst['progress'];tries=inst['attempts'];keys=json.loads(inst['deck_keys'])
            if training_ids is not None:
                if inst['instance_id'] in training_ids:new=task['target_count']
            elif valid and self.condition_passes(task,facts):
                tries+=1;metric=task['metric'];deck_key=','.join(sorted(facts['deck']['cards']))
                if metric=='matches':new+=1
                elif metric=='wins':new+=int(facts['won'])
                elif metric=='towers':new+=facts['stats'].get('towers',0)
                elif metric in ('distinct_decks','distinct_win_decks'):
                    if (metric=='distinct_decks' or facts['won']) and deck_key not in keys:keys.append(deck_key)
                    new=len(keys)
            new=min(new,task['target_count'])
            if new>=task['target_count']:
                balance=self.player(db,p)
                if balance['normal_ap']<act['ap_reserved']:raise GameError('结算余额不足，保留对局重试')
                sid=uid('R');first_count=db.execute('SELECT COUNT(*) FROM first_clears WHERE player_id=? AND season_id=?',(p,b['season_id'])).fetchone()[0]
                first=not db.execute('SELECT 1 FROM first_clears WHERE player_id=? AND season_id=? AND task_id=?',(p,b['season_id'],task['task_id'])).fetchone()
                reward=next(r for r in cfg['rewards'] if r['reward_id']==task['first_reward_id']);first_gold=reward['gold'] if first and first_count<cfg['_rules']['first_reward_limit'] else 0
                db.execute('INSERT INTO settlements VALUES(?,?,?,?,?,?,?,?,?)',(sid,inst['instance_id'],act['activation_id'],b['battle_id'],p,-act['ap_reserved'],act['ap_reserved'],first_gold,self.now()))
                self.change(db,p,'normal_ap',-act['ap_reserved'],'settlement',sid);self.change(db,p,'pass_xp',act['ap_reserved'],'settlement',sid)
                if first:
                    db.execute('INSERT INTO first_clears VALUES(?,?,?,?)',(p,b['season_id'],task['task_id'],sid))
                    if first_gold:self.change(db,p,'gold',first_gold,'first_clear',sid)
                chest=next(x for x in cfg['chests'] if x['chest_id']==task['chest_id']);box=uid('BOX')
                db.execute('INSERT INTO chests(box_id,player_id,bundle_id,chest_id,source_settlement_id,initial_star,created_at) VALUES(?,?,?,?,?,?,?)',(box,p,inst['bundle_id'],task['chest_id'],sid,chest['initial_star'],self.now()))
                self.contribute(db,p,cfg,task,sid)
                db.execute("UPDATE task_instances SET status='completed',progress=?,completed_at=? WHERE instance_id=?",(new,self.now(),inst['instance_id']))
                db.execute("UPDATE activations SET state='committed' WHERE activation_id=?",(act['activation_id'],))
                completed.append({'instance_id':inst['instance_id'],'settlement_id':sid,'box_id':box,'ap_spent':act['ap_reserved']})
            else:
                if task['max_attempts'] and tries>=task['max_attempts']:new=0;tries=0;keys=[]
                db.execute("UPDATE task_instances SET status='accepted',progress=?,attempts=?,deck_keys=? WHERE instance_id=?",(new,tries,dumps(keys),inst['instance_id']))
                db.execute("UPDATE activations SET state='released' WHERE activation_id=?",(act['activation_id'],))
                progress.append({'instance_id':inst['instance_id'],'progress':new})
        result={'battle_id':b['battle_id'],'valid':valid,'completed':completed,'progress':progress}
        db.execute("UPDATE battles SET status='settled',settled_at=?,facts=?,result=? WHERE battle_id=?",(self.now(),dumps(facts),dumps(result),b['battle_id']))
        self.expire(db,p);return result
    def validate_facts(self,f):
        if not isinstance(f,dict) or not isinstance(f.get('won'),bool) or not isinstance(f.get('mode'),str):raise GameError('对局事实缺少模式或胜负',400)
        cards=f.get('deck',{}).get('cards',[])
        if not isinstance(cards,list) or len(cards)!=8 or len(set(cards))!=8 or not all(isinstance(x,str) and len(x)<80 for x in cards):raise GameError('需提交8张不同卡牌的卡组',400)
        for key,value in f.get('stats',{}).items():
            if not isinstance(value,int) or value<0 or value>1000:raise GameError('无效对局统计',400)
        if f.get('stats',{}).get('towers',0)>2:raise GameError('公主塔数量无效',400)
    def contribute(self,db,p,c,task,sid):
        a=self.player(db,p);r=c['_rules'];clan=a['clan_id']
        if not clan or not r['clan_enabled'] or self.now()-a['joined_at']<r['clan_cooldown_days']*86400:return
        base=task['base_contribution'];full=min(base,max(0,r['contribution_threshold']-a['season_contribution']));actual=full+(base-full)*r['contribution_decay']
        db.execute('INSERT INTO contribution_events VALUES(?,?,?,?,?,?,?,?,?)',(uid('CE'),sid,p,clan,a['season_id'],base,actual,actual,self.now()))
        db.execute('INSERT INTO clan_members VALUES(?,?,?,?,?,?) ON CONFLICT(player_id,clan_id,season_id) DO UPDATE SET contribution=contribution+excluded.contribution,completed_tasks=completed_tasks+1',(p,clan,a['season_id'],actual,1,1))
        db.execute('UPDATE players SET season_contribution=season_contribution+? WHERE player_id=?',(actual,p));self.change(db,p,'tokens',actual,'contribution',sid)
    def clan_score(self,db,p,c):
        a=self.player(db,p)
        if not a['clan_id']:return 0
        rows=db.execute('SELECT contribution FROM clan_members WHERE clan_id=? AND season_id=? ORDER BY contribution DESC,player_id',(a['clan_id'],a['season_id'])).fetchall()
        counts=[r[0] for r in db.execute('SELECT COUNT(*) FROM clan_members WHERE season_id=? AND completed_tasks>0 GROUP BY clan_id',(a['season_id'],))]
        # The local group includes four empty demo clans; its benchmark is therefore 30.
        baseline=max(c['_rules']['clan_min_baseline'],min(counts+[0,0,0,0]));return round(sum(r[0]*(1 if rank<baseline else c['_rules']['clan_rank_decay']**(rank-baseline+1)) for rank,r in enumerate(rows)),4)
    def state(self,p):
        with self.tx() as db:
            c=self.config(db);self.recover(db,p,c);self.expire(db,p);self.reconcile_activations(db,p,c);a=self.player(db,p);ap,slots=self.reservations(db,p);pref=self.activation_preferences(db,p)
            instances=[dict(r) for r in db.execute("SELECT * FROM task_instances WHERE player_id=? AND status IN ('accepted','active') ORDER BY accepted_at,instance_id",(p,))]
            tasks=[]
            for i in instances:
                t=json.loads(i['config_snapshot']);helped=bool(db.execute('SELECT 1 FROM task_help_requests WHERE instance_id=?',(i['instance_id'],)).fetchone())
                tasks.append(dict(t,instance_id=i['instance_id'],status=i['status'],progress=i['progress'],help_requested=helped,forced=i['instance_id']==pref['forced_instance_id']))
            tasks.extend(dict(t,instance_id='',status='available',progress=0,help_requested=False) for t in self.displayed(db,p,c))
            boxes=[dict(r) for r in db.execute("SELECT c.box_id,c.chest_id,c.initial_star,COALESCE(o.current_star,c.initial_star) current_star,COALESCE(o.roll_count,0) roll_count FROM chests c LEFT JOIN chest_openings o ON o.box_id=c.box_id WHERE c.player_id=? AND c.status='pending' ORDER BY c.created_at,c.box_id",(p,))]
            a.update(normal_cap=c['_rules']['normal_cap'],reserve_cap=c['_rules']['reserve_cap'],reserved_ap=ap,available_ap=a['normal_ap']-ap,reserved_slots=slots,queue_capacity=c['_rules']['queue_capacity'],pending_chests=len(boxes),accepted_count=len(instances),active_count=sum(i['status']=='active' for i in instances),pass_level=min(c['_rules']['pass_max_level'],a['pass_xp']//c['_rules']['pass_xp_per_level']),clan_score=self.clan_score(db,p,c))
            a.update(activation_order=pref['order_rule'],forced_instance_id=pref['forced_instance_id'] or '')
            battle=db.execute("SELECT battle_id FROM battles WHERE player_id=? AND status='running'",(p,)).fetchone()
            claims=[dict(r) for r in db.execute('SELECT level,track FROM pass_claims WHERE player_id=? AND season_id=?',(p,a['season_id']))]
            helps=[dict(r) for r in db.execute('SELECT help_id,player_id,message FROM task_help_requests WHERE clan_id=? ORDER BY created_at DESC LIMIT 20',(a['clan_id'],))]
            return {'ok':True,'bundle_id':c['_bundle'],'week_id':self.period()[0],'player':a,'tasks':tasks,'chests':boxes,'rewards':c['rewards'],'pass_levels':c['pass_levels'],'pass_claims':claims,'clan_help':helps,'items':[dict(r) for r in db.execute('SELECT resource,amount FROM player_items WHERE player_id=?',(p,))],'battle_id':battle[0] if battle else ''}
