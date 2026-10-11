#!/usr/bin/env python
# -*- coding: utf-8 -*-
exec(open('dis_tryaccept.py').read().split('# 找 GraphUtils')[0])
import json, capstone, bisect
# 全库扫描：所有方法里 [reg+0xa0] 且 reg 是 this(self) 的 —— 通过方法起始 RVA 与偏移范围
d = json.load(open(r'D:/games/Il2CppDumper/script.json', encoding='utf-8'))
S=sorted(M.keys())
alls=[(e['Address'],e['Name']) for e in d.get('ScriptMethod',[]) if e['Address']<0x10000000]
alls.sort()
md2=capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_64)
def endof(a):
    i=bisect.bisect_right(S,a); return S[i] if i<len(S) else a+0x4000
res={}
for a,n in alls:
    ln=endof(a)-a
    if ln>0x8000: continue
    off=rva2off(a)
    for ins in md2.disasm(DATA[off:off+ln], IB+a):
        o=ins.op_str.lower()
        # 只关心 [reg + 0xa0] 形式，且不是 rsp/rbp
        if '+ 0xa0]' in o and 'rsp' not in o and 'rbp' not in o:
            res.setdefault(n,[]).append('0x%x: %s %s'%(ins.address-IB,ins.mnemonic,o))
# 过滤：只保留 "EmporiumEntry" 或相关类
for n,ls in sorted(res.items()):
    if 'EmporiumEntry' in n:
        print('--- %s'%n)
        for l in ls[:12]: print('   ',l)
