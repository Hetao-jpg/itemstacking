#!/usr/bin/env python
# -*- coding: utf-8 -*-
exec(open('dis_tryaccept.py').read().split('# 找 GraphUtils')[0])
import bisect
for name,addr in (('SwapInv',0x7D7BB0),('TransferOwnershipFrontInv',0x7D84C0),('TransferOwnershipBackInv',0x7D8370)):
    print('\n######## %s (0x%X) ########'%(name,addr))
    for l in dis(addr, maxn=140): print(l)
