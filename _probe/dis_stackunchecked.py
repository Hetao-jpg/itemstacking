#!/usr/bin/env python
# -*- coding: utf-8 -*-
exec(open('dis_tryaccept.py').read().split('# 找 GraphUtils')[0])
import json
d = json.load(open(r'D:/games/Il2CppDumper/script.json', encoding='utf-8'))
# 找 GameInventory / GraphNodeStorage 里名字像 Insert / Accept / TryAcceptItem 的方法
for e in d.get('ScriptMethod', []):
    n = e['Name']
    if any(k in n for k in ('GraphNodeStorage$$','GameInventory$$','GameGridInventory$$')) and any(k in n for k in ('Accept','Insert','Stack')):
        print('%08X  %s' % (e['Address'], n))
