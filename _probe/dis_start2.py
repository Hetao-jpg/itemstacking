#!/usr/bin/env python
# -*- coding: utf-8 -*-
exec(open('dis_tryaccept.py').read().split('# 找 GraphUtils')[0])
# 打印 Start 中所有引用字符串的行 + 出现的赋值到 this+offset 的行
S=sorted(M.keys())
import bisect, capstone
def annotate(rva, maxn=1200):
    i=bisect.bisect_right(S,rva); end=S[i] if i<len(S) else rva+0x4000
    off=rva2off(rva)
    for ins in md.disasm(DATA[off:off+(end-rva)], IB+rva):
        t='0x%x: %-9s %s'%(ins.address-IB,ins.mnemonic,ins.op_str)
        # string
        m=re.search(r'\[rip [+] 0x([0-9a-fA-F]+)\]',ins.op_str)
        if m:
            nm=sym(ins.address+ins.size+int(m.group(1),16))
            if nm: t+='   ; << %s'%nm
        # 写入 [rXX + imm] 形式（容器赋值）
        if ins.mnemonic=='mov' and ('[rbx +' in ins.op_str or '[rsi +' in ins.op_str or '[rdi +' in ins.op_str) and ins.op_str.startswith('qword ptr'):
            t+='   *** FIELD WRITE'
        print(t)
annotate(0x7D49A0)
