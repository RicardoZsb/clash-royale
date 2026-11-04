import html
import json
import re
from pathlib import Path

p = Path(__file__).parent
s = (p / 'figjam-source.xml').read_text(encoding='utf-8')
sections = []
for attrs, body in re.findall(r'<section\b([^>]*)>(.*?)</section>', s, re.S):
    meta = dict(re.findall(r'(\w+)="([^"]*)"', attrs))
    texts = []
    for kind, attrs2, content in re.findall(r'<(shape-with-text|text|sticky|connector)\b([^>]*)>(.*?)</\1>', body, re.S):
        node = dict(re.findall(r'(\w+)="([^"]*)"', attrs2))
        text = html.unescape(re.sub(r'<[^>]+>', '', content)).strip()
        if text:
            texts.append({'type': kind, 'id': node.get('id'), 'text': text,
                          'from': node.get('connectorStart'), 'to': node.get('connectorEnd')})
    sections.append({'id': meta.get('id'), 'name': meta.get('name'), 'texts': texts})
(p / 'figjam-text.json').write_text(json.dumps(sections, ensure_ascii=False, indent=2), encoding='utf-8')
print(json.dumps([{'id': x['id'], 'name': x['name'], 'textNodes': len(x['texts'])} for x in sections], ensure_ascii=False, indent=2))
