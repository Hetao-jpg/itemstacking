#!/usr/bin/env python
# -*- coding: utf-8 -*-
exec(open('dis_tryaccept.py').read().split('# 找 GraphUtils')[0])
for name, rva in [('GameGridInventory.UncheckedAccept',0x77C370),('SlotMarker.StackItemUnchecked',0x7AFDF0),('SlotMarker.AcceptUnchecked',0x7AFC30)]:
    print('\n===== %s (0x%X) =====' % (name, rva))
    for l in dis(rva, maxn=200):
        print(l)
