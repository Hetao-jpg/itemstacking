#!/usr/bin/env python
# -*- coding: utf-8 -*-
exec(open('dis_tryaccept.py').read().split('# 找 GraphUtils')[0])
for name, rva in [('GraphUtils.TryAcceptOnce',0x766730),('SlotMarker.TryAcceptOnce',0x7B0020),('GraphUtils.TryAcceptAll',0x766650),('GraphUtils.TryAcceptAllOrDestroy',0x766570)]:
    print('\n===== %s (0x%X) =====' % (name, rva))
    for l in dis(rva, maxn=140):
        print(l)
