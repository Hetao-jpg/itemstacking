#!/usr/bin/env python
# -*- coding: utf-8 -*-
exec(open('dis_tryaccept.py').read().split('# 找 GraphUtils')[0])
print('===== SlotMarker.AcceptUnchecked (0x7AFC30) =====')
for l in dis(0x7AFC30, maxn=80):
    print(l)
print()
# 谁调用 AcceptUnchecked / StackItemUnchecked？扫 GameAssembly 里的 call 目标
import re, bisect
print('--- 扫描 call 0x1807AFC30 / 0x1807AFDF0 的调用点 ---')
for target,label in ((0x7AFC30,'SlotMarker.AcceptUnchecked'),(0x7AFDF0,'SlotMarker.StackItemUnchecked'),(0x7B0020,'SlotMarker.TryAcceptOnce')):
    hits=[]
    for rva in S:
        i=bisect.bisect_right(S,rva); end=S[i] if i<len(S) else rva+0x2000
        if end-rva>0x4000: end=rva+0x4000
        off=rva2off(rva)
        if off is None: continue
        code=DATA[off:off+(end-rva)]
        if not code: continue
        for ins in md.disasm(code, IB+rva):
            if ins.mnemonic=='call' and ins.op_str.startswith('0x'):
                try: va=int(ins.op_str,16)
                except: continue
                if va==IB+target or va==target:
                    hits.append(M.get(rva,'???'))
    print('%-28s 被 %d 处调用: %s' % (label, len(hits), sorted(set(hits))[:12]))
