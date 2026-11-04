import fs from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { Workbook, SpreadsheetFile } from '@oai/artifact-tool';
const root=path.dirname(fileURLToPath(import.meta.url));
const seed=JSON.parse(await fs.readFile(path.join(root,'Config/design-seed.json'),'utf8'));
const output=path.resolve(root,'../Assets/Tables/ClashRoyale_V5.4.xlsx');
const preview=path.join(root,'Validation');await fs.mkdir(preview,{recursive:true});
const wb=Workbook.create();
const col=n=>{let s='';while(n){n--;s=String.fromCharCode(65+n%26)+s;n=Math.floor(n/26);}return s;};
const sheets=[];
function table(name,headers,rows,widths,subtitle){
  const s=wb.worksheets.add(name);sheets.push(s);s.showGridLines=false;s.tabColor='#406572';
  const end=col(headers.length),last=rows.length+4;
  s.getRange(`A1:${end}${last}`).format={font:{name:'Microsoft YaHei',size:11,color:'#253642'},verticalAlignment:'center',wrapText:true,rowHeight:36};
  s.getRange(`A1:${end}1`).merge();s.getRange('A1').values=[[name]];s.getRange(`A1:${end}1`).format={fill:'#253F4C',font:{name:'Microsoft YaHei',size:20,bold:true,color:'#FFFFFF'},rowHeight:42};
  s.getRange(`A2:${end}2`).merge();s.getRange('A2').values=[[subtitle]];s.getRange(`A2:${end}2`).format={font:{color:'#536674',size:10},rowHeight:32};
  s.getRange(`A4:${end}${last}`).values=[headers,...rows];
  s.getRange(`A4:${end}4`).format={fill:'#496B78',font:{bold:true,color:'#FFFFFF'},rowHeight:34};
  s.getRange(`A5:${end}${last}`).format.font.color='#195889';
  for(let i=0;i<headers.length;i++)s.getRange(`${col(i+1)}1:${col(i+1)}${last}`).format.columnWidthPx=widths[i]||130;
  s.tables.add(`A4:${end}${last}`,true,'Config_'+sheets.length);
  for(let r=5;r<=last;r+=2)s.getRange(`A${r}:${end}${r}`).format.fill='#F0F5F6';
  s.freezePanes.freezeRows(4);s.freezePanes.freezeColumns(1);
  return s;
}
const overview=table('使用与总览',['项目','数量','操作'],[
 ['一级任务',null,'修改任务表中的简述与详细描述，分别对应卡片收起和展开状态。'],
 ['二级任务',null,'任务 ID 保持唯一，奖励引用必须存在。'],
 ['三级任务',null,'condition_json 使用条件字典中的字段。'],
 ['特殊任务',null,'max_attempts 为0表示不限有效对局次数。'],
 ['任务总数',null,'保存 Excel 后，在 Unity 菜单「Clash Royale / 本机后端联调」中重新导入。'],
 ['战令等级',null,'已接取任务保留原配置；新接取任务读取新导入版本。'],
 ['运行流程',null,'接取 → 激活预留 → 对局达成扣点 → 宝箱入队 → 手动开箱或战令领奖。']
],[150,100,620],'任务配表 / 资源规则 / 奖励配置；所有表头位于第4行，数据从第5行开始。');
const taskHeaders=['task_id','title','brief','description','tier','metric','target_count','condition_json','max_attempts','action_cost','chest_id','first_reward_id','base_contribution','audience','enabled'];
const taskNames=['一级任务','二级任务','三级任务','特殊任务'];
for(let i=0;i<4;i++){
 const rows=seed.tasks.filter(t=>t.tier===i+1).map(t=>taskHeaders.map(k=>t[k]));
 const s=table(taskNames[i],taskHeaders,rows,[105,190,245,385,80,150,125,360,125,120,130,140,160,185,105],`简述 brief：收起状态显示；详情 description：展开后显示。等级 ${i+1}。`);
 s.getRange(`A5:O${rows.length+4}`).format.rowHeight=74;
 s.getRange(`A5:A${rows.length+4}`).setNumberFormat('@');
 s.getRange(`K5:L${rows.length+4}`).setNumberFormat('@');
 s.getRange(`F5:F${rows.length+4}`).dataValidation={rule:{type:'list',values:['matches','wins','towers','distinct_decks','distinct_win_decks']}};
 s.getRange(`O5:O${rows.length+4}`).dataValidation={rule:{type:'list',values:['TRUE','FALSE']}};
}
const ruleDescription={queue_capacity:'待开启宝箱与有效预留箱位的总上限',cancel_gold_per_tower:'超过每日免费次数后，取消费用＝国王塔等级×此值',contribution_threshold:'账号赛季贡献超过此值后，超出部分应用衰减倍率',extra_ap_season_budget:'每赛季额外来源行动点的合计上限'};
table('行动点与规则',['key','value','description'],seed.rules.map(r=>[r.key,r.value,ruleDescription[r.key]||r.description]),[240,120,530],'数值配置；行动点接取不扣、激活预留、任务达成实扣。');
table('奖励',['reward_id','gold','cards','ap'],seed.rewards.map(r=>[r.reward_id,r.gold,r.cards,r.ap]),[160,140,140,140],'金币、卡牌资源和行动点均使用非负整数。');
table('宝箱',['chest_id','initial_star'],seed.chests.map(r=>[r.chest_id,r.initial_star]),[210,210],'任务等级决定初始星级；开启时读取行动点与规则中的升星配置。');
table('星级奖励',['star','reward_id'],seed.stars.map(r=>[r.star,r.reward_id]),[210,210],'开启后的最终星级对应奖励 ID。');
table('战令40级',['level','required_xp','free_reward_id','premium_reward_id'],seed.pass_levels.map(r=>[r.level,r.required_xp,r.free_reward_id,r.premium_reward_id]),[120,180,200,220],'经验达到门槛后手动领取；每级每条轨道仅可领取一次。');
table('任务池配额',['tier','weekly_count','daily_count'],seed.pool.map(r=>[r.tier,r.weekly_count,r.daily_count]),[180,210,210],'每周62条；每天20条；已接取任务保留到完成、取消或周结束。');
table('条件字典',['key','meaning'],seed.conditions.map(r=>[r.key,r.meaning]),[280,570],'布尔条件使用 true/false；以 _min 或 _max 结尾的条件使用非负数值。');
table('配置版本',['key','value'],[['version',seed.version],['season_id',seed.season_id]],[210,400],'修改后导入生成新的不可变配置包，任务实例保存接取时的配置快照。');
for(let i=0;i<4;i++)overview.getRange(`B${i+5}`).formulas=[[`=COUNTA('${taskNames[i]}'!A5:A${seed.tasks.filter(t=>t.tier===i+1).length+4})`]];
overview.getRange('B9').formulas=[['=SUM(B5:B8)']];overview.getRange('B10').formulas=[["=COUNTA('战令40级'!A5:A44)"]];
overview.getRange('A9:C9').format={fill:'#DBEACD',font:{bold:true,color:'#234036'}};
wb.recalculate();
const totals=overview.getRange('B5:B10').values;if(JSON.stringify(totals)!==JSON.stringify([[25],[20],[16],[1],[62],[40]]))throw Error('Count formulas failed: '+JSON.stringify(totals));
const scan=await wb.inspect({kind:'match',searchTerm:'#REF!|#DIV/0!|#VALUE!|#NAME\\?|#N/A',options:{useRegex:true,maxResults:10},maxChars:1500});
await fs.writeFile(path.join(preview,'workbook-checks.json'),JSON.stringify({totals,scan},null,2));
await fs.mkdir(path.dirname(output),{recursive:true});const xlsx=await SpreadsheetFile.exportXlsx(wb);await xlsx.save(output);
console.log(JSON.stringify({output,sheets:sheets.length,totals}));
for(const s of process.argv.includes('--render')?sheets:[]){
 const last=taskNames.includes(s.name)?8:Math.min(s.getUsedRange().rowCount,11);
 const range=s.name==='使用与总览'?'A1:C11':taskNames.includes(s.name)?`A1:D${last}`:`A1:${s.name==='战令40级'?'D':s.name==='奖励'?'D':s.name==='行动点与规则'||s.name==='任务池配额'?'C':'B'}${last}`;
 const blob=await wb.render({sheetName:s.name,range,scale:1,format:'png'});await fs.writeFile(path.join(preview,s.name+'.png'),new Uint8Array(await blob.arrayBuffer()));
}
