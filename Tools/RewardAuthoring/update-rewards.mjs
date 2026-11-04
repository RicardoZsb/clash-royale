import fs from 'node:fs/promises';
import {FileBlob,SpreadsheetFile} from '@oai/artifact-tool';
const path='../../Assets/Tables/ClashRoyale_V5.4.xlsx';
console.log('Importing workbook');
const book=await SpreadsheetFile.importXlsx(await FileBlob.load(path));
console.log('Workbook imported');
if(process.argv.includes('--preview')){
 const im=await book.render({sheetName:'战令40级',range:'A1:D12',scale:1,format:'png'});
 await fs.writeFile('../../DesignReference/Validation/Rewards/table-before.png',new Uint8Array(await im.arrayBuffer()));
 console.log((await book.inspect({kind:'sheet',include:'id,name',maxChars:1200})).ndjson);process.exit(0);
}
const tables=JSON.parse(await fs.readFile('original-tables.json','utf8'));
const free=[['gold',500],['gems',10],['chest',2],['epic',5],['legendary',1],['ap',1],['common',100],['gold',750],['rare',20],['chest',3],['gems',15],['common',150],['ap',1],['gold',1000],['epic',8],['rare',25],['gold',1250],['chest',2],['gems',20],['chest',4],['ap',1],['common',200],['gold',1500],['rare',30],['legendary',1],['gems',25],['epic',10],['gold',1750],['ap',1],['chest',4],['rare',40],['gold',2000],['common',250],['gems',30],['epic',15],['ap',1],['gold',2500],['rare',50],['legendary',2],['chest',5]];
const paid=[['common',250],['rare',25],['chest',4],['gems',25],['chest',5],['gold',1500],['rare',40],['common',350],['gold',2000],['legendary',2],['epic',15],['gems',35],['gold',2500],['rare',50],['chest',4],['common',450],['gold',3000],['epic',20],['gems',40],['chest',5],['rare',60],['gold',3500],['common',550],['gems',50],['legendary',3],['gold',4000],['epic',25],['rare',75],['common',650],['chest',5],['gems',60],['gold',4500],['rare',90],['epic',30],['legendary',4],['common',750],['gold',5000],['gems',75],['epic',40],['chest',5]];
const rewards=tables['奖励'].map(r=>({...r,resource:'',amount:0,chest_star:0}));
for(let i=0;i<40;i++)for(const [track,schedule] of [['FREE',free],['PREMIUM',paid]]){
 const [kind,amount]=schedule[i],id=`PASS_${track}_${String(i+1).padStart(2,'0')}`;
 const r={...rewards[0],reward_id:id,gold:0,cards:0,ap:0,resource:'',amount:0,chest_star:0};
 // Keep the existing reward name column, if present, as the actual resource name.
 if('name' in r)r.name={gold:'金币',gems:'宝石',common:'普通万能卡',rare:'稀有万能卡',epic:'史诗万能卡',legendary:'传奇万能卡',ap:'行动点',chest:'宝箱'}[kind];
 if(kind==='gold'||kind==='ap')r[kind]=amount;else if(kind==='chest')r.chest_star=amount;else{r.resource=kind;r.amount=amount;}
 rewards.push(r);tables['战令40级'][i][track==='FREE'?'free_reward_id':'premium_reward_id']=id;
}
tables['奖励']=rewards;tables['行动点与规则'].find(r=>r.key==='upgrade_attempts').value=4;
for(const name of ['奖励','战令40级','行动点与规则']){
 const sheet=book.worksheets.getItem(name),rows=tables[name],headers=Object.keys(rows[0]);
 const col=n=>{let s='';while(n){s=String.fromCharCode(65+(n-1)%26)+s;n=Math.floor((n-1)/26);}return s;};
 const last=col(headers.length);sheet.getRange(`A4:${last}${rows.length+4}`).values=[headers,...rows.map(r=>headers.map(h=>r[h]??''))];
 if(name==='奖励'){
  sheet.getRange(`A5:${last}${rows.length+4}`).format.font={name:'Microsoft YaHei',size:11};
  sheet.getRange(`A4:${last}4`).format={fill:'#26394D',font:{name:'Microsoft YaHei',size:11,bold:true,color:'#FFFFFF'}};
  sheet.getRange('E:G').format.columnWidth=20;
 }
}
book.recalculate();await(await SpreadsheetFile.exportXlsx(book)).save(path);
await fs.writeFile('../../DesignReference/Validation/Rewards/pass-schedule.json',JSON.stringify({free,paid},null,2));
if(process.argv.includes('--render')){
 const im=await book.render({sheetName:'战令40级',range:'A1:D12',scale:1,format:'png'});
 await fs.writeFile('../../DesignReference/Validation/Rewards/table-after.png',new Uint8Array(await im.arrayBuffer()));
}
console.log('Updated 40 free and 40 premium rewards, four chest rolls.');
