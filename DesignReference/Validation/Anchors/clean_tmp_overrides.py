import json,re
from pathlib import Path
folder=Path(__file__).parent
manifest=json.loads((folder/'tmp-overrides-to-remove.json').read_text(encoding='utf-8'))
scene=Path(manifest['scenePath'])
text=scene.read_bytes().decode('utf-8')
parts=re.split(r'(?=^--- !u!\d+ &)',text,flags=re.M)
removed=0
for i,block in enumerate(parts):
    header=re.match(r'--- !u!\d+ &(\d+)',block)
    if not header:continue
    for entry in manifest['entries']:
        if entry['prefabInstanceBlockId']!=header[1]:continue
        pattern=(r'^    - target: '+re.escape(entry['target'])+r'\r?\n'
                 r'      propertyPath: '+re.escape(entry['propertyPath'])+r'\r?\n'
                 r'      value: '+re.escape(entry['currentSceneOverrideValue'])+r'\r?\n'
                 r'      objectReference: '+re.escape(entry['objectReference'])+r'\r?\n')
        block,n=re.subn(pattern,'',block,flags=re.M)
        assert n==1,(header[1],entry['target'],entry['propertyPath'],n)
        removed+=n
    parts[i]=block
assert removed==manifest['count']==146,removed
(folder/'scene-before-tmp-cleanup.txt').write_bytes(scene.read_bytes())
scene.write_bytes(''.join(parts).encode('utf-8'))
print(json.dumps({'removedNewTmpOverrides':removed,'scene':str(scene)}))
