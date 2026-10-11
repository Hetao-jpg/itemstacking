#!/usr/bin/env python
# -*- coding: utf-8 -*-
exec(open('dis_tryaccept.py').read().split('# 找 GraphUtils')[0])
def raw(rva, n):
    off=rva2off(rva); out=[]
    for ins in md.disasm(DATA[off:off+n], IB+rva):
        t='0x%x: %-9s %s'%(ins.address-IB,ins.mnemonic,ins.op_str)
        m=re.search(r'\[rip [+] 0x([0-9a-fA-F]+)\]',ins.op_str)
        if m:
            nm=sym(ins.address+ins.size+int(m.group(1),16))
            if nm: t+='   ; << %s'%nm[:70]
        out.append(t)
    return out
print('=== 0x98 区域 (backInvinvElement) ===')
for l in raw(0x7d5a80, 0xA0): print(l)
print()
print('=== 0xA0 区域 (backInvinvElementCounter) ===')
for l in raw(0x7d5c20, 0xA0): print(l)
