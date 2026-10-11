#!/usr/bin/env python
# -*- coding: utf-8 -*-
exec(open('dis_tryaccept.py').read().split('# 找 GraphUtils')[0])
import bisect
S=sorted(M.keys())
def window(center, lo, hi):
    i=bisect.bisect_right(S,center); end=S[i] if i<len(S) else center+0x4000
    off=rva2off(center-lo)
    start=center-lo+IB
    print('--- around 0x%X ---'%(center))
    for ins in md.disasm(DATA[off:off+(hi-lo)], start):
        t='0x%x: %-9s %s'%(ins.address-IB,ins.mnemonic,ins.op_str)
        m=re.search(r'\[rip [+] 0x([0-9a-fA-F]+)\]',ins.op_str)
        if m:
            nm=sym(ins.address+ins.size+int(m.group(1),16))
            if nm: t+='   ; << %s'%nm[:70]
        print(t)
# 0x7d4f6f 写 [rdi+0x88]，前面是 "Node FrontInvWindow"(0x7d5008?) — 看两个窗口
window(0x7d4f6f, 0x140, 0x40)
window(0x7d5008, 0xC0, 0x120)
