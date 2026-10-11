#!/usr/bin/env python
# -*- coding: utf-8 -*-
exec(open('dis_tryaccept.py').read().split('# 找 GraphUtils')[0])
import json
d = json.load(open(r'D:/games/Il2CppDumper/script.json', encoding='utf-8'))
# 找 EmporiumEntry 的所有方法（按 RVA 范围 0x7C9000 ~ 0x7DA000）
ents=[]
for e in d.get('ScriptMethod', []):
    a=e['Address']
    if e['Name'].startswith('EmporiumEntry$$') and 0x7C9000 <= a <= 0x7DB000:
        ents.append((a, e['Name']))
ents.sort()
print('EmporiumEntry 方法总数:', len(ents))
for a,n in ents:
    print('%08X  %s' % (a, n))
