#!/usr/bin/env python
# -*- coding: utf-8 -*-
exec(open('dis_tryaccept.py').read().split('# 找 GraphUtils')[0])
import json, capstone, bisect
d = json.load(open(r'D:/games/Il2CppDumper/script.json', encoding='utf-8'))
ents=[]
for e in d.get('ScriptMethod', []):
    a=e['Address']
    if e['Name'].startswith('EmporiumEntry$$') and 0x7C9000 <= a <= 0x7DB000:
        ents.append((a, e['Name']))
ents.sort()
md=capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_64)
S=sorted(M.keys())
hits={}
for a,n in ents:
    i=bisect.bisect_right(S,a); end=S[i] if i<len(S) else a+0x2000
    off=rva2off(a); ln=end-a
    for ins in md.disasm(DATA[off:off+ln], IB+a):
        o=ins.op_str.lower()
        if '+ 0xa0]' in o:
            hits.setdefault(n,[]).append('0x%x: %s %s'%(ins.address-IB,ins.mnemonic,ins.op_str))
for n,ls in sorted(hits.items()):
    print('--- %s' % n)
    for l in ls[:20]: print('   ',l)
print('\n== 命中方法数:', len(hits))
