const fs=require('fs'),path=require('path'),cp=require('child_process');
const root=path.resolve(__dirname,'..'),out=path.join(root,'Assets/ClashRoyaleUI/Art');fs.mkdirSync(out,{recursive:true});
const files=fs.readdirSync(__dirname).filter(x=>x.endsWith('.tsx'));const urls={},idMap={},hashMap={},screens=[];
for(const file of files){const src=fs.readFileSync(path.join(__dirname,file),'utf8'),prefix=(src.match(/const assetPathPrefix = "([^"]+)"/)||[])[1];const vars={};for(const m of src.matchAll(/const (\w+) = `\$\{assetPathPrefix\}\/([^`]+)`;/g)){vars[m[1]]=m[2];urls[m[2]]=prefix+'/'+m[2];}
 for(const m of src.matchAll(/src=\{(\w+)\}/g)){const pre=src.slice(0,m.index),ids=[...pre.matchAll(/data-node-id="([^"]+)"/g)];if(ids.length)idMap[ids.at(-1)[1]]=vars[m[1]];}
 const name=file.slice(0,-4),d=JSON.parse(fs.readFileSync(path.join(__dirname,name+'.flat.json'),'utf8'));d.key=name;screens.push(d);for(const n of d.nodes)for(const f of n.fills||[])if(f.imageHash&&idMap[n.id])hashMap[f.imageHash]=idMap[n.id];
}
fs.writeFileSync(path.join(__dirname,'asset-map.json'),JSON.stringify({urls,idMap,hashMap},null,2));
fs.writeFileSync(path.join(__dirname,'screens.json'),JSON.stringify({screens},null,2));
const missing=[];for(const s of screens)for(const n of s.nodes)for(const f of n.fills||[])if(f.imageHash&&!hashMap[f.imageHash])missing.push({screen:s.key,id:n.id,name:n.name,hash:f.imageHash});
fs.writeFileSync(path.join(__dirname,'unmapped-assets.json'),JSON.stringify(missing,null,2));console.log(JSON.stringify({screens:screens.length,assets:Object.keys(urls).length,mappedHashes:Object.keys(hashMap).length,missing}));
async function main(){const jobs=Object.entries(urls);let index=0;async function worker(){while(index<jobs.length){const [name,url]=jobs[index++];const target=path.join(out,name);if(fs.existsSync(target))continue;await new Promise((resolve,reject)=>cp.execFile('curl.exe',['-sS','-f','-L','--retry','2','-o',target,url],(err)=>err?reject(err):resolve()));}}await Promise.all(Array.from({length:6},worker));const sharp=require('C:/Users/Administrator/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');for(const name of Object.keys(urls).filter(n=>n.endsWith('.svg'))){await sharp(path.join(out,name),{density:288}).png().toFile(path.join(out,name+'.png'));}console.log('Downloaded all '+jobs.length+' design assets.');}
main().catch(e=>{console.error(e);process.exitCode=1;});
