#!/usr/bin/env python
# -*- coding: utf-8 -*-
exec(open('dis_tryaccept.py').read().split('# 找 GraphUtils')[0])
def raw(rva, n):
    off=rva2off(rva)
    for ins in md.disasm(DATA[off:off+n], IB+rva):
        t='0x%x: %-9s %s'%(ins.address-IB,ins.mnemonic,ins.op_str)
        m=re.search(r'\[rip [+] 0x([0-9a-fA-F]+)\]',ins.op_str)
        if m:
            nm=sym(ins.address+ins.size+int(m.group(1),16))
            if nm: t+='   ; << %s'%nm[:70]
        print(t)
# Start 从 0x7D49A0 到 0x7D7BF0；直接从头 dump 到 rdi+0xA0 之后
raw(0x7D5008, 0x1200)
