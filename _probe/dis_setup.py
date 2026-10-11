#!/usr/bin/env python
# -*- coding: utf-8 -*-
exec(open('dis_tryaccept.py').read().split('# 找 GraphUtils')[0])
import json
d = json.load(open(r'D:/games/Il2CppDumper/script.json', encoding='utf-8'))
want = ('EmporiumEntry$$get_backInvinvElementCounter','EmporiumEntry$$get_frontInvinvElement',
        'EmporiumEntry$$get_backInvinvElement','EmporiumEntry$$set_backInvinvElementCounter',
        'EmporiumEntry$$set_frontInvinvElement','EmporiumEntry$$set_backInvinvElement',
        'EmporiumEntry$$GetCountersItems','EmporiumEntry$$IsFrontCounterEmpty')
tg = {}
for e in d.get('ScriptMethod', []):
    if e['Name'] in want: tg[e['Name']] = e['Address']
for n,a in sorted(tg.items(), key=lambda x:x[1]):
    print('\n===== %s (0x%X) =====' % (n, a))
    for l in dis(a, maxn=80): print(l)
