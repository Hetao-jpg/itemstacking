#!/usr/bin/env python
# -*- coding: utf-8 -*-
exec(open('dis_tryaccept.py').read().split('# 找 GraphUtils')[0])
for name, addr in (('ShowFrontCounters',0x7D4670),('HideFrontCounters',0x7CEE60),
                   ('ShowFrontWindow',0x7D46F0),('ShowFrontArea',0x7D4510),
                   ('ResetFrontCounterGrid',0x7D38B0),('ExpandFrontCounterGrid',0x7CB3B0)):
    print('\n######## %s (0x%X) ########' % (name, addr))
    for l in dis(addr, maxn=90): print(l)
