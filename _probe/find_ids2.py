#!/usr/bin/env python
# -*- coding: utf-8 -*-
exec(open('dis_tryaccept.py').read().split('# 找 GraphUtils')[0])
import json,re
d=json.load(open(SCRIPT,encoding='utf-8'))
pat=re.compile(r'(front_inv|back_inv|counter|save_bag|storage|display|bazar|hidden|sold|swap)', re.I)
seen=set()
for e in d.get('ScriptString',[]):
    v=e['Value']
    if not isinstance(v,str) or len(v)<3 or len(v)>44: continue
    if pat.search(v) and ' ' not in v and not v.startswith('__'):
        seen.add(v)
for v in sorted(seen): print(v)
