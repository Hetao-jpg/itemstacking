#!/usr/bin/env python
# -*- coding: utf-8 -*-
exec(open('dis_tryaccept.py').read().split('# 找 GraphUtils')[0])
# Start 里所有字符串字面量
raw=open(BIN,'rb').read()
S2={}
for e in json.load(open(SCRIPT,encoding='utf-8')).get('ScriptString',[]):
    S2[e['Address']]=e['Value']
# 直接扫 Start 范围里的 lea rcx,[rip+..] / mov r?,[rip+..] 指向的字符串
import bisect
a0=0x7D49A0; a1=0x7D7BF0
off=rva2off(a0)
seen=set()
for ins in md.disasm(DATA[off:off+(a1-a0)], IB+a0):
    m=re.search(r'\[rip [+] 0x([0-9a-fA-F]+)\]',ins.op_str)
    if not m: continue
    va=ins.address+ins.size+int(m.group(1),16)
    for k in (va, va-IB):
        if k in S2:
            v=S2[k]
            if v not in seen:
                seen.add(v); print('%-28s : %s' % ('0x%x'%(ins.address-IB), v[:70]))
            break
