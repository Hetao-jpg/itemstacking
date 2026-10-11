#!/usr/bin/env python
# -*- coding: utf-8 -*-
exec(open('dis_tryaccept.py').read().split('# 找 GraphUtils')[0])
import json
d = json.load(open(r'D:/games/Il2CppDumper/script.json', encoding='utf-8'))
tg = {}
for e in d.get('ScriptMethod', []):
    n = e['Name']
    if n in ('EmporiumEntry$$GetCountersItems','EmporiumEntry$$IsFrontCounterEmpty','EmporiumEntry$$ShowFrontCounters','EmporiumEntry$$ExpandFrontCounterGrid','EmporiumEntry$$ResetFrontCounterGrid'):
        tg[n] = e['Address']
for n,a in sorted(tg.items(), key=lambda x:x[1]):
    print('\n===== %s (0x%X) =====' % (n, a))
    for l in dis(a, maxn=120):
        print(l)
