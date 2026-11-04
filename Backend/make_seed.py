"""Creates the initial editable design data. XLSX is the subsequent source of truth."""
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parent
tasks = []

def task(tier, title, brief, description, metric='matches', target=1, condition=None, source='新增设计', audience='NORMAL', attempts=0):
    index = sum(t['tier'] == tier for t in tasks) + 1
    tasks.append(dict(task_id=str(110000+tier*1000+index), tier=tier, title=title, brief=brief,
        description=description, metric=metric, target_count=target, condition_json=json.dumps(condition or {},ensure_ascii=False,separators=(',',':')),
        max_attempts=attempts, action_cost=min(tier,3), chest_id=str(600000+tier), first_reward_id='700001',
        base_contribution=[10,25,45,60][tier-1], audience=audience, enabled=True, source=source))

S='V5.4 案例'; N='NORMAL,NEW'; R='NORMAL,RETURN'
task(1,'已保存卡组对局','用已保存卡组完成2场','使用已保存卡组完成2场有效对局。','matches',2,{'saved':True},S)
task(1,'冷门卡组尝试','用近期最少使用的卡组对局','使用最近30场中出场最少的已保存卡组完成1场有效对局。',condition={'least_used':True,'saved':True},source=S)
task(1,'试用卡组初体验','选择试用卡组完成1场','从提供的3套等级统一试用卡组中选择1套，完成1场有效对局。',condition={'trial':True},source=S,audience=N)
task(1,'首场有效对局','完成1场有效对局','完成1场符合任务模式要求的有效对局。投降、掉线和消极对局不计入进度。',source=S,audience=N)
task(1,'部队部署','单场部署5次部队','在一场有效对局中部署至少5次部队卡。',condition={'troop_plays_min':5},source=S,audience=N)
task(1,'法术配合','单场使用部队与法术','在同一场有效对局中至少使用1次部队卡和1次法术卡。',condition={'troop_plays_min':1,'spell_plays_min':1},source=S,audience=N)
task(1,'攻破公主塔','累计摧毁1座公主塔','在有效对局中累计摧毁1座公主塔。','towers',1,source=S,audience=N)
task(1,'推荐卡组练习','推荐卡组完成2场','使用系统推荐卡组完成2场有效对局。','matches',2,{'recommended':True},S,N)
task(1,'调整一张卡','调整推荐卡组后完成1场','在推荐卡组中替换至少1张卡后，完成1场有效对局。',condition={'recommended':True,'replaced_min':1},source=S,audience=N)
task(1,'重拾旧卡组','旧常用卡组完成1场','使用回归前最常用且当前合法的卡组完成1场有效对局。',condition={'old_main':True},source=S,audience=R)
task(1,'熟悉版本推荐','版本推荐卡组完成1场','使用当前版本推荐卡组完成1场有效对局。',condition={'recommended':True},source=S,audience=R)
task(1,'部落卡组借鉴','查看并使用部落卡组','查看部落成员分享的卡组并采用该卡组完成1场有效对局。',condition={'clan_shared':True,'viewed_shared':True},source=S,audience=R)
task(1,'双卡组练习','两套不同卡组各完成1场','使用2套不同卡组各完成1场有效对局；只调整卡牌排列不视为新卡组。','distinct_decks',2,source=S,audience=R)
task(1,'法术卡组练习','双法术卡组完成1场','使用至少含2张法术卡的已保存卡组完成1场有效对局。',condition={'saved':True,'spells_min':2})
task(1,'建筑卡组练习','建筑卡组完成1场','使用至少含1张建筑卡的已保存卡组完成1场有效对局。',condition={'saved':True,'buildings_min':1})
task(1,'轻费循环练习','均费不超过3完成1场','使用平均圣水消耗不超过3.0的卡组完成1场有效对局。',condition={'elixir_max':3.0})
task(1,'双法术施放','单场使用法术2次','在一场有效对局中使用至少2次法术卡。',condition={'spell_plays_min':2})
task(1,'建筑部署','单场部署建筑2次','在一场有效对局中部署至少2次建筑卡。',condition={'building_plays_min':2})
task(1,'后备卡组对局','非主力卡组完成1场','使用非主力卡组完成1场有效对局。',condition={'non_main':True})
task(1,'新卡组探索','近期未用卡组完成1场','使用最近30场未使用过的卡组完成1场有效对局。',condition={'unused_30':True})
task(1,'稳步推进','累计摧毁2座公主塔','在有效对局中累计摧毁2座公主塔。','towers',2)
task(1,'持续练习','完成3场有效对局','完成3场有效对局。','matches',3)
task(1,'存档卡组攻塔','保存卡组摧毁1座公主塔','使用已保存卡组在有效对局中累计摧毁1座公主塔。','towers',1,{'saved':True},S,R)
task(1,'三类卡配合','单场使用三类卡牌','在一场有效对局中使用部队、法术和建筑卡各至少1次。',condition={'troop_plays_min':1,'spell_plays_min':1,'building_plays_min':1})
task(1,'卡组微调练习','主力替换2张后对局','在主力卡组中替换至少2张不同卡牌后完成1场有效对局。',condition={'replaced_min':2})

task(2,'法术卡组取胜','使用法术卡组赢得1场','使用至少含2张法术卡的已保存卡组赢得1场有效对局。','wins',1,{'saved':True,'spells_min':2},S)
task(2,'建筑卡组取胜','使用建筑卡组赢得1场','使用至少含1张建筑卡的已保存卡组赢得1场有效对局。','wins',1,{'saved':True,'buildings_min':1},S)
task(2,'替换主力卡取胜','主力替换2张后获胜','在主力卡组中替换至少2张不同卡牌，赢得1场有效对局。','wins',1,{'replaced_min':2},S)
task(2,'轻费卡组取胜','均费不超过3赢得1场','使用平均圣水消耗不超过3.0的卡组赢得1场有效对局。','wins',1,{'elixir_max':3.0},S)
task(2,'陌生卡组首胜','近期未用卡组赢得1场','使用最近30场未使用过的卡组赢得1场有效对局。','wins',1,{'unused_30':True},S)
task(2,'推荐卡组首胜','推荐卡组赢得1场','使用系统推荐卡组赢得1场有效对局。','wins',1,{'recommended':True},S,N)
task(2,'试用卡组首胜','试用卡组赢得1场','从提供的等级统一试用卡组中选择1套，赢得1场有效对局。','wins',1,{'trial':True},S,N)
task(2,'建筑法术协作','推荐混合卡组赢得1场','使用同时含建筑和法术的推荐卡组赢得1场有效对局。','wins',1,{'recommended':True,'buildings_min':1,'spells_min':1},S,N)
task(2,'同类型卡牌调整','替换两张同类卡后对局','在推荐卡组中替换至少2张同类型卡牌，完成1场有效对局。','matches',1,{'recommended':True,'replaced_min':2,'same_type_replacements':True},S,N)
task(2,'两种试用流派','两套试用卡组各完成1场','采用2套不同流派的等级统一试用卡组，各完成1场有效对局。','distinct_decks',2,{'trial':True},S,N)
task(2,'版本变化体验','新卡试用卡组赢得1场','使用包含新卡或本版本调整卡的试用卡组赢得1场有效对局。','wins',1,{'trial':True,'changed_card':True},S,R)
task(2,'旧卡组焕新','调整旧主力后赢得1场','按版本推荐替换旧主力卡组至少2张卡，赢得1场有效对局。','wins',1,{'old_main':True,'replaced_min':2,'recommended':True},S,R)
task(2,'部落方案首胜','部落分享卡组赢得1场','使用部落成员分享的卡组赢得1场有效对局。','wins',1,{'clan_shared':True},S,R)
task(2,'备用卡组首胜','非主力卡组赢得1场','使用非主力卡组赢得1场有效对局。','wins',1,{'non_main':True})
task(2,'双卡组首胜','两套不同卡组各赢1场','使用2套不同卡组各赢得1场有效对局。','distinct_win_decks',2)
task(2,'低频卡组首胜','近期最少用的卡组获胜','使用最近30场出场最少的已保存卡组赢得1场有效对局。','wins',1,{'least_used':True,'saved':True})
task(2,'攻塔训练','保存卡组累计摧毁3塔','使用已保存卡组在有效对局中累计摧毁3座公主塔。','towers',3,{'saved':True})
task(2,'试用攻塔','试用卡组累计摧毁3塔','使用等级统一试用卡组在有效对局中累计摧毁3座公主塔。','towers',3,{'trial':True})
task(2,'混合卡组演练','建筑法术卡组完成3场','使用同时包含建筑与法术的已保存卡组完成3场有效对局。','matches',3,{'saved':True,'buildings_min':1,'spells_min':1})
task(2,'连续积累胜场','已保存卡组累计赢2场','使用已保存卡组累计赢得2场有效对局，无需连胜。','wins',2,{'saved':True})

task(3,'跨卡组累计胜场','非主力卡组累计赢2场','使用非主力卡组累计赢得2场有效对局。','wins',2,{'non_main':True},S)
task(3,'陌生推荐挑战','4场内推荐卡组赢2场','使用陌生推荐卡组，在4场有效对局内赢得2场；未达成时重置本轮进度。','wins',2,{'recommended':True,'non_main':True},S,R,4)
task(3,'双法术进阶','双法术卡组累计赢3场','使用至少含2张法术卡的已保存卡组累计赢得3场有效对局。','wins',3,{'saved':True,'spells_min':2})
task(3,'建筑体系进阶','建筑卡组累计赢3场','使用至少含1张建筑卡的已保存卡组累计赢得3场有效对局。','wins',3,{'saved':True,'buildings_min':1})
task(3,'轻费体系进阶','轻费卡组累计赢3场','使用均费不超过3.0的卡组累计赢得3场有效对局。','wins',3,{'elixir_max':3.0})
task(3,'双卡组进阶','两套非主力卡组各获胜','使用2套不同的非主力卡组各赢得1场有效对局。','distinct_win_decks',2,{'non_main':True})
task(3,'三卡组挑战','三套不同卡组各获胜','使用3套不同卡组各赢得1场有效对局。','distinct_win_decks',3)
task(3,'试用进阶','试用卡组累计赢3场','使用等级统一试用卡组累计赢得3场有效对局。','wins',3,{'trial':True})
task(3,'冷门体系进阶','低频卡组累计赢2场','使用最近30场出场最少的已保存卡组累计赢得2场有效对局。','wins',2,{'saved':True,'least_used':True})
task(3,'新体系实践','近期未用卡组累计赢2场','使用本轮激活前最近30场未使用的卡组累计赢得2场有效对局。','wins',2,{'unused_30':True})
task(3,'部落方案进阶','部落分享卡组累计赢3场','使用部落成员分享卡组累计赢得3场有效对局。','wins',3,{'clan_shared':True})
task(3,'大幅调整挑战','替换3张主力卡赢2场','在主力卡组中替换至少3张卡牌后累计赢得2场有效对局。','wins',2,{'replaced_min':3})
task(3,'混合体系进阶','建筑法术卡组赢3场','使用同时包含建筑与法术的已保存卡组累计赢得3场有效对局。','wins',3,{'saved':True,'buildings_min':1,'spells_min':1})
task(3,'备用卡组攻塔','非主力卡组累计摧毁6塔','使用非主力卡组在有效对局中累计摧毁6座公主塔。','towers',6,{'non_main':True})
task(3,'试用攻塔进阶','试用卡组累计摧毁6塔','使用等级统一试用卡组在有效对局中累计摧毁6座公主塔。','towers',6,{'trial':True})
task(3,'版本推荐进阶','推荐卡组累计赢3场','使用当前版本推荐卡组累计赢得3场有效对局。','wins',3,{'recommended':True})
task(4,'主题试用挑战','5场内试用卡组赢3场','从3套主题试用卡组中选择1套，在5场有效对局内赢得3场；未达成时重置本轮进度。','wins',3,{'trial':True,'theme_trial':True},S,'NORMAL',5)

rules = [
('normal_cap',24,'V5.4','普通行动点上限'),('reserve_cap',16,'V5.4','储备池上限'),('recovery_hours',6,'V5.4','恢复间隔小时'),('normal_recovery',2,'V5.4','未满池每周期恢复'),('reserve_recovery',1,'V5.4','普通池满时每周期储备增长'),
('queue_capacity',20,'V5.4 正文优先','FigJam 旧案例容量8不采用'),('first_reward_limit',100,'V5.4','每赛季模板首次奖励上限'),('pass_xp_per_level',5,'V5.4','每级所需经验'),('pass_max_level',40,'用户要求','战令总等级'),('daily_free_cancels',3,'V5.4','每日免费取消次数'),
('cancel_gold_per_tower',10,'演示假设','收费公式为塔等级×此值；正文未定'),('clan_cooldown_days',3,'V5.4','入部落冷却'),('contribution_threshold',1000,'演示假设','正式衰减阈值待定'),('contribution_decay',0.5,'V5.4','超阈值部分倍率'),('clan_rank_decay',0.85,'V5.4','超基准人数后的权重'),('clan_min_baseline',30,'V5.4','计分人数基准下限'),
('clan_reward_min',40,'V5.4','竞赛领奖最低贡献'),('honor_min',200,'V5.4','前10名荣誉最低贡献'),('shop_ap_price',100,'V5.4','1点行动点的代币价格'),('shop_ap_season_limit',10,'V5.4','赛季商店行动点购买上限'),('extra_ap_season_budget',50,'演示假设','赛季额外行动点预算待定'),
('upgrade_attempts',5,'V5.4','每次开启升星判定次数'),('upgrade_probability',0.3,'V5.4','每次升星概率'),('max_star',5,'用户UI规则','1到5星'),('tasks_enabled',1,'本机默认','停用后禁止新接取激活；锁定对局可结算'),('pass_enabled',1,'本机默认','战令开关'),('clan_enabled',1,'本机默认','部落开关')]
rewards=[dict(reward_id='700001',gold=100,cards=0,ap=0,source='V5.4 演算值'),dict(reward_id='700002',gold=100,cards=5,ap=0,source='V5.4 演算值'),dict(reward_id='700004',gold=0,cards=0,ap=1,source='V5.4 演算值')]
for star,(g,c) in enumerate([(50,2),(100,5),(200,10),(400,20),(800,40)],1):
    if star!=2:rewards.append(dict(reward_id=str(710000+star),gold=g,cards=c,ap=0,source='演示假设'))
rewards.append(dict(reward_id='720001',gold=500,cards=25,ap=0,source='演示假设'))
chests=[dict(chest_id=str(600000+i),initial_star=i) for i in range(1,5)]
stars=[dict(star=i,reward_id='700002' if i==2 else str(710000+i)) for i in range(1,6)]
levels=[dict(level=i,required_xp=i*5,free_reward_id='700004' if i==1 else stars[(i-1)%5]['reward_id'],premium_reward_id='720001',source='V5.4 演算值' if i==1 else '演示假设') for i in range(1,41)]
pool=[dict(tier=i,weekly_count=w,daily_count=d) for i,(w,d) in enumerate([(25,8),(20,6),(16,5),(1,1)],1)]
conditions=[dict(key=k,meaning=v) for k,v in [ ('saved','卡组已经保存'),('least_used','最近30场中最低使用频次'),('trial','等级统一试用卡组'),('recommended','推荐卡组'),('replaced_min','相对基准卡组至少替换卡数'),('spells_min','卡组最少法术数'),('buildings_min','卡组最少建筑数'),('elixir_max','卡组最大平均圣水'),('troop_plays_min','单场最少部队部署次数'),('spell_plays_min','单场最少法术使用次数'),('building_plays_min','单场最少建筑部署次数'),('non_main','非主力卡组'),('old_main','回归前主力'),('unused_30','本轮激活前最近30场未使用'),('clan_shared','部落分享卡组'),('viewed_shared','已查看对应部落分享'),('same_type_replacements','替换卡牌同类型'),('changed_card','含新卡或改动卡'),('theme_trial','主题试用卡组')]]
data=dict(version='V5.4-local-001',season_id='202610',tasks=tasks,rules=[dict(key=k,value=v,source=s,description=d) for k,v,s,d in rules],rewards=rewards,chests=chests,stars=stars,pass_levels=levels,pool=pool,conditions=conditions)
assert len(tasks)==62 and [sum(t['tier']==i for t in tasks) for i in range(1,5)]==[25,20,16,1]
(ROOT/'Config').mkdir(exist_ok=True)
(ROOT/'Config'/'design-seed.json').write_text(json.dumps(data,ensure_ascii=False,indent=2),encoding='utf-8')
print('Created 62 task templates (25/20/16/1), 40 pass levels; source labels retained.')
