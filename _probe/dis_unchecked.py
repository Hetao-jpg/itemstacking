#!/usr/bin/env python
# -*- coding: utf-8 -*-
exec(open('dis_tryaccept.py').read().split('# 找 GraphUtils')[0])
for name, rva in [('GameGridInventory.UncheckedAccept',0x77C370),('GameInventory.UncheckedAcceptAll',0x784680),('GameInventory.CheckAcceptParentShapeStateModifier',0x7831F0),('SlotMarker.StackItemUnchecked',0x7AFDF0)]:
    print('\n===== %s (0x%X) =====' % (name, rva))
    for l in dis(rva, maxn=90):
        print(l)
