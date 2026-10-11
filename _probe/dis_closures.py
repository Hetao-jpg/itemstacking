#!/usr/bin/env python
# -*- coding: utf-8 -*-
exec(open('dis_tryaccept.py').read().split('# 找 GraphUtils')[0])
import json
d=json.load(open(SCRIPT,encoding='utf-8'))
S2={e['Address']:e['Value'] for e in d.get('ScriptString',[])}
targets=[]
for e in d.get('ScriptMethod',[]):
    n=e['Name']
    if 'EmporiumEntry.<>c.<Start>b__182_' in n or 'EmporiumEntry.<Start>b__182_' in n:
        targets.append((e['Address'],n))
targets.sort()
for a,n in targets:
    # dump 少量指令找字符串
    off=rva2off(a)
    strs=[]
    for ins in md.disasm(DATA[off:off+0x200], IB+a):
        m=re.search(r'\[rip [+] 0x([0-9a-fA-F]+)\]',ins.op_str)
        if m:
            va=ins.address+ins.size+int(m.group(1),16)
            for k in (va,va-IB):
                if k in S2: strs.append(S2[k]); break
        if ins.mnemonic=='ret': break
    print('%-40s %s' % (n.split('EmporiumEntry.')[-1], ' | '.join(dict.fromkeys(strs))[:110]))
