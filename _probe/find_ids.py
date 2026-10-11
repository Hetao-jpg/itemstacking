#!/usr/bin/env python
# -*- coding: utf-8 -*-
exec(open('dis_tryaccept.py').read().split('# 找 GraphUtils')[0])
import json,re
# 搜索所有 ScriptString 里像容器 id 的（含 back_inv / front_inv / backpack / storage 等）
d=json.load(open(SCRIPT,encoding='utf-8'))
pat=re.compile(r'(inv|backpack|storage|bag|container|save|bay|counter|case|shelf|box|pocket)', re.I)
seen=set()
for e in d.get('ScriptString',[]):
    v=e['Value']
    if not isinstance(v,str) or len(v)<3 or len(v)>40: continue
    if pat.search(v) and v.islower() and ' ' not in v:
        if v not in seen:
            seen.add(v)
for v in sorted(seen): print(v)
