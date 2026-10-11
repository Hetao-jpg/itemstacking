#!/usr/bin/env python
# -*- coding: utf-8 -*-
exec(open('dis_tryaccept.py').read().split('# 找 GraphUtils')[0])
for l in dis(0x7AFDF0, maxn=300):
    print(l)
