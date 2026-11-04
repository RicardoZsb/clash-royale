"""Read exported XLSX without Excel/Office or third-party Python packages."""
import argparse,json,re,zipfile,xml.etree.ElementTree as ET
from pathlib import Path

NS={'m':'http://schemas.openxmlformats.org/spreadsheetml/2006/main','r':'http://schemas.openxmlformats.org/officeDocument/2006/relationships'}
TASK_SHEETS=['一级任务','二级任务','三级任务','特殊任务']
TABLES={'行动点与规则':'rules','奖励':'rewards','宝箱':'chests','星级奖励':'stars','战令40级':'pass_levels','任务池配额':'pool','条件字典':'conditions'}
METRICS={'matches','wins','towers','distinct_decks','distinct_win_decks'}
CONDITIONS={'saved','least_used','trial','recommended','replaced_min','spells_min','buildings_min','elixir_max','troop_plays_min','spell_plays_min','building_plays_min','non_main','old_main','unused_30','clan_shared','viewed_shared','same_type_replacements','changed_card','theme_trial'}

def read_tables(path):
    with zipfile.ZipFile(path) as z:
        strings=[]
        if 'xl/sharedStrings.xml' in z.namelist():
            strings=[''.join(n.itertext()) for n in ET.fromstring(z.read('xl/sharedStrings.xml')).findall('m:si',NS)]
        rels={e.attrib['Id']:e.attrib['Target'] for e in ET.fromstring(z.read('xl/_rels/workbook.xml.rels'))}
        sheets={}
        for s in ET.fromstring(z.read('xl/workbook.xml')).findall('m:sheets/m:sheet',NS):
            target=rels[s.attrib['{'+NS['r']+'}id']];target=target.lstrip('/') if target.startswith('/') else 'xl/'+target
            rows={}
            for cell in ET.fromstring(z.read(target)).findall('.//m:sheetData/m:row/m:c',NS):
                match=re.fullmatch(r'([A-Z]+)(\d+)',cell.attrib['r']);col=0
                for ch in match[1]:col=col*26+ord(ch)-64
                row=int(match[2]);typ=cell.attrib.get('t');v=cell.find('m:v',NS)
                if typ=='inlineStr':value=''.join(cell.find('m:is',NS).itertext())
                elif v is None:value=None
                elif typ=='s':value=strings[int(v.text)]
                elif typ=='b':value=v.text=='1'
                elif typ in ('str','e'):value=v.text
                else:
                    value=float(v.text);value=int(value) if value.is_integer() else value
                rows.setdefault(row,{})[col]=value
            headers=rows.get(4,{})
            sheets[s.attrib['name']]=[{str(h):rows[r].get(col) for col,h in headers.items() if h is not None} for r in sorted(rows) if r>4 and rows[r].get(1) is not None]
        return sheets

def from_xlsx(path):
    sheets=read_tables(path);meta={r['key']:r['value'] for r in sheets['配置版本']}
    data={'version':str(meta['version']),'season_id':str(meta['season_id']),'tasks':sum([sheets[s] for s in TASK_SHEETS],[])}
    data.update({key:sheets[name] for name,key in TABLES.items()})
    for t in data['tasks']:
        for key in ['task_id','chest_id','first_reward_id']:t[key]=str(t[key])
        if isinstance(t['enabled'],str):
            if t['enabled'].lower() not in ('true','false'):raise ValueError('enabled 必须为 TRUE 或 FALSE')
            t['enabled']=t['enabled'].lower()=='true'
    for name,keys in [('rewards',['reward_id']),('chests',['chest_id']),('stars',['reward_id']),('pass_levels',['free_reward_id','premium_reward_id'])]:
        for row in data[name]:
            for key in keys:row[key]=str(row[key])
    validate(data);return data

def validate(c):
    def check(ok,msg):
        if not ok:raise ValueError(msg)
    def unique(rows,key):check(len({r[key] for r in rows})==len(rows),key+' 重复')
    check(bool(c['version']) and bool(c['season_id']),'缺少版本/赛季')
    ts=c['tasks'];check(len(ts)==62,'任务总数必须为62');unique(ts,'task_id')
    check([sum(t['tier']==i for t in ts) for i in range(1,5)]==[25,20,16,1],'任务分级必须25/20/16/1')
    for name,key in [('rules','key'),('rewards','reward_id'),('chests','chest_id'),('stars','star'),('pass_levels','level'),('pool','tier')]:unique(c[name],key)
    rules={r['key']:r['value'] for r in c['rules']}
    required={'normal_cap','reserve_cap','recovery_hours','normal_recovery','reserve_recovery','queue_capacity','first_reward_limit','pass_xp_per_level','pass_max_level','daily_free_cancels','cancel_gold_per_tower','clan_cooldown_days','contribution_threshold','contribution_decay','clan_rank_decay','clan_min_baseline','clan_reward_min','honor_min','shop_ap_price','shop_ap_season_limit','extra_ap_season_budget','upgrade_attempts','upgrade_probability','max_star','tasks_enabled','pass_enabled','clan_enabled'}
    check(required<=rules.keys(),'规则字段缺失')
    check(all(isinstance(v,(int,float)) and v>=0 for v in rules.values()),'规则必须为非负数')
    check(all(isinstance(v,int) for k,v in rules.items() if k not in ('contribution_decay','clan_rank_decay','upgrade_probability')),'容量、次数、点数等规则必须为整数')
    check(rules['max_star']==5 and rules['pass_max_level']==40,'本项目使用5星宝箱和40级战令')
    for k in ('normal_cap','reserve_cap','recovery_hours','normal_recovery','reserve_recovery','queue_capacity','pass_xp_per_level'):check(rules[k]>0,k+' 必须大于0')
    for k in ('contribution_decay','clan_rank_decay','upgrade_probability'):check(rules[k]<=1,k+' 必须在0至1之间')
    rewards={r['reward_id']:r for r in c['rewards']};chests={r['chest_id']:r for r in c['chests']}
    for r in rewards.values():check(all(isinstance(r[k],int) and r[k]>=0 for k in ('gold','cards','ap')),'奖励必须为非负整数')
    for r in rewards.values():
        check(r.get('resource','') in ('',None,'gems','common','rare','epic','legendary'),'未知奖励资源')
        check(isinstance(r.get('amount',0),int) and r.get('amount',0)>=0,'奖励数量必须为非负整数')
        check(isinstance(r.get('chest_star',0),int) and 0<=r.get('chest_star',0)<=5,'宝箱星级必须在0至5之间')
    for t in ts:
        check(all(isinstance(t[k],str) and t[k].strip() for k in ('title','brief','description')),'任务文本不能为空')
        check(t['metric'] in METRICS,'不支持的进度类型');check(isinstance(t['target_count'],int) and t['target_count']>0,'目标必须为正整数')
        check(t['action_cost']==min(t['tier'],3),'任务成本与等级不一致');check(t['base_contribution']==[10,25,45,60][t['tier']-1],'贡献与等级不一致')
        check(isinstance(t['enabled'],bool),'enabled 必须布尔值');check(t['chest_id'] in chests and t['first_reward_id'] in rewards,'任务奖励引用不存在')
        check(chests[t['chest_id']]['initial_star']==t['tier'],'宝箱初始星级与任务等级不一致')
        check(rewards[t['first_reward_id']]['cards']==0 and rewards[t['first_reward_id']]['ap']==0,'首次奖励只允许金币')
        check(set(t['audience'].split(','))<={'NORMAL','NEW','RETURN'},'玩家分层无效')
        cond=json.loads(t['condition_json']);check(isinstance(cond,dict) and set(cond)<=CONDITIONS,'卡组条件字段无效')
        for k,v in cond.items():check(isinstance(v,(int,float)) and not isinstance(v,bool) and v>=0 if k.endswith(('_min','_max')) else isinstance(v,bool),'条件值类型错误：'+k)
        check(isinstance(t['max_attempts'],int) and t['max_attempts']>=0,'尝试次数无效')
    check(sorted(r['star'] for r in c['stars'])==list(range(1,int(rules['max_star'])+1)),'星级奖励缺失')
    check(all(r['reward_id'] in rewards for r in c['stars']),'星级奖励引用错误')
    check(sorted(r['level'] for r in c['pass_levels'])==list(range(1,41)),'战令必须完整1到40级')
    for r in c['pass_levels']:
        check(r['required_xp']==r['level']*rules['pass_xp_per_level'],'战令经验应为等级乘每级经验')
        check(r['free_reward_id'] in rewards and r['premium_reward_id'] in rewards,'战令奖励引用错误')
        check(rewards[r['premium_reward_id']]['ap']==0,'付费战令不能产出行动点')
    check(sorted((r['tier'],r['weekly_count'],r['daily_count']) for r in c['pool'])==[(1,25,8),(2,20,6),(3,16,5),(4,1,1)],'任务池配额不符合V5.4')
    return c

if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('xlsx');parser.add_argument('--db');parser.add_argument('--json');args=parser.parse_args()
    config=from_xlsx(args.xlsx)
    if args.json:Path(args.json).write_text(json.dumps(config,ensure_ascii=False,indent=2),encoding='utf-8')
    if args.db:
        from game import Game
        print(Game(args.db).install(config,args.xlsx))
    print('Validated: 62 tasks; 25/20/16/1; 40 pass levels; references and rules OK')
