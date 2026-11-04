import json, copy
from pathlib import Path

folder = Path(__file__).parent
pages = {p['screen']: p['rects'] for p in json.loads((folder/'before-structures.json').read_text(encoding='utf-8-sig'))}
plans = {}

for screen, rows in pages.items():
    bypath = {r['path']: r for r in rows}
    byid = {r['id']: r for r in rows if r['id']}
    changes = {}
    def anchor(r, lo, hi=None):
        if isinstance(r, str): r = bypath[r] if r in bypath else byid[r]
        hi = hi or lo
        parent = bypath.get(r['path'].rsplit('/',1)[0]) if r['path'] else None
        wh = [parent['w'], parent['h']] if parent else [402,874]
        oldref = [r['min'][i]+(r['max'][i]-r['min'][i])*r['pivot'][i] for i in range(2)]
        newref = [lo[i]+(hi[i]-lo[i])*r['pivot'][i] for i in range(2)]
        pos = [r['p'][i]+(oldref[i]-newref[i])*wh[i] for i in range(2)]
        size = [r['s'][i]+((r['max'][i]-r['min'][i])-(hi[i]-lo[i]))*wh[i] for i in range(2)]
        changes[r['path']] = dict(path=r['path'], min=lo, max=hi, p=pos, s=size)
    def ids(values, lo, hi=None):
        for value in values.split(): anchor(value,lo,hi)
    def fill(path): anchor(path,[0,0],[1,1])
    def xtop(path): anchor(path,[0,1],[1,1])
    def xbottom(path): anchor(path,[0,0],[1,0])
    def children(path): return [r for r in rows if r['path'] and r['path'].rsplit('/',1)[0]==path]
    for path in ['', '/Content','/Header','/BottomNavigation']:
        if path in bypath: fill(path)
    for r in rows:
        if r['path']=='/Border': fill(r['path'])
        if r['path'].startswith('/BottomNavigation/'):
            name=r['sourceName'] or ''
            if name=='底部导航': xbottom(r['path'])
            elif name=='底边栏背景板': xbottom(r['path'])
            else:
                for title,x in [('商店',0),('卡组',.25),('主页',.5),('部落',.75),('锦标赛',1)]:
                    if name.startswith(title): anchor(r,[x,0]); break
    if screen=='BattleHome':
        for r in children('/Content'):
            if r['id'] in ['26:57','27:69','27:68']: anchor(r,[.5,.5])
            elif r['y']>=575: anchor(r,[.5,0])
            elif r['x']>=237: anchor(r,[1,1])
            else: anchor(r,[0,1])
        ids('6:7 16:49 7:14 16:48',[1,1])
        ids('6:9 6:4 7:16 16:47',[.5,1])
    elif screen=='ClanHome':
        anchor('30:118',[.5,0],[.5,1]); xtop(byid['30:102']['path'])
        ids('30:104 36:3 145:5 30:108',[.5,1])
        ids('30:96 36:4 30:116 30:114 30:112 30:110 145:3 145:4',[1,1])
        fill(byid['145:2']['path']); ids('145:39 145:40 145:41',[.5,0])
        anchor('/MemberScrollView',[.5,0],[.5,1])
    elif screen=='Tasks':
        fill(byid['70:196']['path']); ids('71:202 71:203 71:205',[.5,1])
        ids('70:173 70:176',[.5,1]); ids('70:175 70:178',[1,1])
        fill('/TaskScrollView'); xtop('/TaskScrollView/Viewport/Content')
        for id in ['71:213','81:306','81:307']: xtop(byid[id]['path'])
        ids('130:5 130:6 130:7 130:8 130:9 130:10 130:13 130:14 130:15',[1,1])
    elif screen=='BattlePass':
        fill(byid['59:58']['path']); anchor('158:73',[.5,0],[.5,1]); fill(byid['59:98']['path'])
        anchor('158:79',[.5,1]); anchor('158:72',[.5,0]); xtop(byid['158:80']['path'])
        for r in rows:
            if r['path'].endswith('/RewardScrollView'): anchor(r,[.5,0],[.5,1])
        for r in children(byid['158:80']['path']):
            x=0 if r['x']+r['w']/2<140 else (1 if r['x']+r['w']/2>265 else .5)
            anchor(r,[x,1])
    elif screen=='ChestQueue':
        fill(byid['71:214']['path']); ids('71:219 71:220 71:222 70:180 70:183',[.5,1])
        ids('70:182 70:185',[1,1])
        for r in rows:
            if r['path'] in ['/Merge_3_Star_Pair_To_4_Star','/Open_2_Star_Chest']: anchor(r,[.5,1])
    elif screen=='ChestReveal':
        fill(byid['62:123']['path']); anchor('63:132',[.5,.5]); ids('63:129 63:130 63:131 63:133',[.5,0])
    elif screen in ['ClanShop','ClanCompetition','ClanChat','ClanChatInput']:
        for r in children('/Header'):
            if r['w']>380: xtop(r['path'])
            elif r['x']>300: anchor(r,[1,1])
            elif r['x']>100: anchor(r,[.5,1])
        if screen=='ClanShop':
            fill(byid['79:286']['path']); xtop(byid['79:293']['path']); fill('/ShopCategoryContent')
            for id,y in [('79:288',.75),('79:289',.5),('79:292',.25),('79:290',0)]: anchor(id,[0,y],[0,y+.25])
            for id,y in [('82:336',.875),('81:334',.625),('82:337',.375),('82:338',.125)]: anchor(id,[0,y])
            xtop('/ShopCategoryContent/Viewport/Content')
        elif screen=='ClanCompetition':
            for r in children('/Content'): anchor(r,[.5,.5])
        elif screen=='ClanChat':
            fill(byid['79:287']['path'])
            ids('76:240 81:308',[0,0]); ids('76:241 81:304 81:305',[.4,0])
            ids('76:242 81:300',[.7,0]); ids('81:297 81:296',[1,0])
            xtop('/LocalMessage'); xtop('/LocalMessage/Message')
        else:
            fill(byid['79:294']['path']); xbottom(byid['76:247']['path'])
            ids('81:301 81:302 76:269',[1,0])
            fill(byid['76:247']['path']+'/TextArea'); fill(byid['76:247']['path']+'/TextArea/Text')
    # Full-screen, non-rendering popup containers let blockers cover all aspect ratios.
    for root,point in [('/TaskSortDropdown',[1,1]),('/TaskActivationDropdown',[.5,0])]:
        if root not in bypath: continue
        def direct(path, lo, hi, p, s): changes[path]=dict(path=path,min=lo,max=hi,p=p,s=s)
        for path in [root,root+'/Popup',root+'/Popup/OutsideClickToClose']:
            direct(path,[0,0],[1,1],[0,0],[0,0])
        for r in children(root)+[bypath[root+'/Popup/OptionsPanel']]:
            if r['path'].endswith('/Popup'): continue
            # These children retain their reference-screen positions under the expanded containers.
            pos=[r['x']-402*point[0], -r['y']+874*(1-point[1])]
            direct(r['path'],point,point,pos,[r['w'],r['h']])
    plans[screen]=changes

components = []
for name,screen,key in [
    ('BottomNavigation','BattlePass','158:81'),('RewardTabBar','BattlePass','158:80'),
    ('PassOverview','BattlePass','158:79'),('ChestMergePair','ChestQueue','/Merge_3_Star_Pair_To_4_Star'),
    ('ClanChatInputField','ClanChatInput','76:247'),
    ('TaskSortDropdown','Tasks','/TaskSortDropdown'),('TaskActivationDropdown','BattleHome','/TaskActivationDropdown')]:
    rows=pages[screen]
    prefix=key if key.startswith('/') else next(r['path'] for r in rows if r['id']==key)
    selected=[]
    for path,change in plans[screen].items():
        if path==prefix or path.startswith(prefix+'/'):
            c=copy.deepcopy(change); c['path']=path[len(prefix):]; selected.append(c)
    assert selected, (name,prefix)
    components.append(dict(asset=f'Assets/ClashRoyaleUI/Prefabs/Components/{name}.prefab',changes=selected))
out=components+[dict(asset=f'Assets/ClashRoyaleUI/Prefabs/Screens/{s}.prefab',changes=list(p.values())) for s,p in plans.items()]
(folder/'anchor-plan.json').write_text(json.dumps(out,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps({p['asset'].split('/')[-1]:len(p['changes']) for p in out},ensure_ascii=False))
