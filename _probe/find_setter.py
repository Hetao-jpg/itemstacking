#!/usr/bin/env python
# -*- coding: utf-8 -*-
exec(open('dis_tryaccept.py').read().split('# 找 GraphUtils')[0])
import json, bisect, re
d = json.load(open(r'D:/games/Il2CppDumper/script.json', encoding='utf-8'))
# 找 setter / getter 的 ScriptMethod 条目（含具体名字）
targets = {}
for e in d.get('ScriptMethod', []):
    n = e['Name']
    if 'set_backInvinvElementCounter' in n or 'set_showcaseElement' in n or 'set_backInvinvElement' in n or 'set_soldElement' in n:
        targets[n] = e['Address']
print('目标 setter 地址:')
for n,a in sorted(targets.items()):
    print('  %08X  %s' % (a, n))
