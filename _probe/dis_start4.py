#!/usr/bin/env python
# -*- coding: utf-8 -*-
exec(open('dis_tryaccept.py').read().split('# 找 GraphUtils')[0])
import bisect
S=sorted(M.keys())
def window(center, lo, hi):
    off=rva2off(center-lo)
    print('--- around 0x%X ---'%(center))
    for ins in md.disasm(DATA[off:off+(hi-lo)], IB+center-lo):
        t='0x%x: %-9s %s'%(ins.address-IB,ins.mnemonic,ins.op_str)
        m=re.search(r'\[rip [+] 0x([0-9a-fA-F]+)\]',ins.op_str)
        if m:
            nm=sym(ins.address+ins.size+int(m.group(1),16))
            if nm: t+='   ; << %s'%nm[:70]
        print(t)
window(0x7d5083, 0x160, 0x140)
