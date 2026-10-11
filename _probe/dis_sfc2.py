#!/usr/bin/env python
# -*- coding: utf-8 -*-
exec(open('dis_tryaccept.py').read().split('# 找 GraphUtils')[0])
print('###### ShowFrontCounters (0x7D4670) ######')
for l in dis(0x7D4670, maxn=70): print(l)
print()
print('###### GetBackInvItems (0x7CDE80) ######')
for l in dis(0x7CDE80, maxn=70): print(l)
print()
print('###### GetInvItems (0x7CE310) ######')
for l in dis(0x7CE310, maxn=70): print(l)
