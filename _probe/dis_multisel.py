#!/usr/bin/env python
# -*- coding: utf-8 -*-
"""dis ItemMultiSelectHandler.OnEventPress / UpdateDropTarget / EndGroupDrag 的 Alt 分支。"""
import json, re, bisect, capstone, pefile

BIN = r'D:\SteamLibrary\steamapps\common\Probably Stolen Demo\GameAssembly.dll'
SCRIPT = r'D:\games\Il2CppDumper\script.json'
pe = pefile.PE(BIN, fast_load=True); IB = pe.OPTIONAL_HEADER.ImageBase
SECS = [(s.VirtualAddress, max(s.Misc_VirtualSize, s.SizeOfRawData), s.PointerToRawData) for s in pe.sections]
DATA = open(BIN, 'rb').read()
def rva2off(rva):
    for va, sz, praw in SECS:
        if va <= rva < va+sz: return praw + (rva-va)
    return None
d = json.load(open(SCRIPT, encoding='utf-8'))
M = {e['Address']: e['Name'] for e in d.get('ScriptMethod', []) if e['Address'] < 0x10000000}
MM = {e['Address']: e['Name'] for e in d.get('ScriptMetadataMethod', []) if e['Address'] < 0x10000000}
STRS = {e['Address']: e['Value'] for e in d.get('ScriptString', [])}
S = sorted(M.keys())
def sym(va):
    for k in (va, va - IB):
        for t in (M, MM, STRS):
            if k in t:
                v = t[k]; return '"%s"' % v[:60] if t is STRS else v
    return None
JMP = ('call','jmp','jne','je','ja','jb','jbe','jae','jg','jl','jge','jle','js','jns','jz','jnz')
md = capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_64); md.detail = True
def dis(rva, maxn=400):
    i = bisect.bisect_right(S, rva)
    end = S[i] if i < len(S) else rva+0x2000
    off = rva2off(rva); out=[]
    for n, ins in enumerate(md.disasm(DATA[off:off+(end-rva)], IB+rva)):
        t = '0x%x:  %-9s %s' % (ins.address, ins.mnemonic, ins.op_str)
        if ins.mnemonic in JMP:
            m2 = re.search(r'0x([0-9a-fA-F]+)$', ins.op_str)
            if m2:
                nm = sym(int(m2.group(1),16))
                if nm: t += '   ; <- ' + nm
        for m2 in re.finditer(r'\[rip [+] 0x([0-9a-fA-F]+)\]', ins.op_str):
            nm = sym(ins.address+ins.size+int(m2.group(1),16))
            if nm: t += '   ; [rip] = %s' % nm; break
        out.append(t)
        if n+1>=maxn or ins.mnemonic=='ret': break
    return out

for name, rva in [('OnEventPress',0x7de060), ('UpdateDropTarget',0x7dff50), ('EndGroupDrag',None)]:
    if rva is None:
        hits=[a for a,n in M.items() if n.endswith('ItemMultiSelectHandler$$EndGroupDrag')]
        rva=hits[0] if hits else None
    if rva is None: continue
    print('\n===== %s (0x%X) =====' % (name, rva))
    lines = dis(rva)
    for l in lines:
        if any(k in l for k in ('GetKey','0x134','0x130','0x131','0x132','0x25a','GetAlt','Alt')):
            print(l)
    print('  (total %d insns)' % len(lines))
