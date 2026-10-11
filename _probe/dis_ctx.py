#!/usr/bin/env python
# -*- coding: utf-8 -*-
exec(open('dis_tryaccept.py').read().split('# 找 GraphUtils')[0])
print('\n############ GetAllOwnedItemsShallow (0x7CC9E0) ############')
for l in dis(0x7CC9E0, maxn=200): print(l)
