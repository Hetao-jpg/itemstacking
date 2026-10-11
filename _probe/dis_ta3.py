#!/usr/bin/env python
# -*- coding: utf-8 -*-
"""GraphUtils.TryAcceptOnce + GameInventory 的 vtable slot 0x2d8/0x2e0 + TryFindOneValidInventorySlot"""
import json, re, bisect, capstone, pefile, sys
BIN = r'D:/SteamLibrary/steamapps/common/Probably Stolen Demo\GameAssembly.dll'
SCRIPT = r'D:/games/Il2CppDumper/script.json'
pe = pefile.PE(BIN, fast_load=True); IB = pe.OPTIONAL_HEADER.ImageBase
SECS = [(s.VirtualAddress, max(s.Misc_VirtualSize, s.SizeOfRawData), s.PointerToRawData) for s in pe.sections]
DATA = open(BIN, 'rb').read()
def rva2off(rva):
    for va, sz, praw in SECS:
        if va <= rva < va+sz: return praw+(rva-va)
d = json.load(open(SCRIPT, encoding='utf-8'))
M = {e['Address']: e['Name'] for e in d.get('ScriptMethod', []) if e['Address']<0x10000000}
MM = {e['Address']: e['Name'] for e in d.get('ScriptMetadataMethod', []) if e['Address']<0x10000000}
STRS = {e['Address']: e['Value'] for e in d.get('ScriptString', [])}
S = sorted(M.keys())
def sym(va):
    for k in (va, va-IB):
        for t in (M, MM, STRS):
            if k in t:
                v=t[k]; return '"%s"'%v[:60] if t is STRS else v
    return None
JMP=('call','jmp','jne','je','ja','jb','jbe','jae','jg','jl','jge','jle','js','jns','jz','jnz')
md=capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_64); md.detail=True
def dis(rva, maxn=300):
    i=bisect.bisect_right(S,rva); end=S[i] if i<len(S) else rva+0x2000
    off=rva2off(rva)
    for n,ins in enumerate(md.disasm(DATA[off:off+(end-rva)], IB+rva)):
        t='0x%x:  %-9s %s'%(ins.address,ins.mnemonic,ins.op_str)
        if ins.mnemonic in JMP:
            m2=re.search(r'0x([0-9a-fA-F]+)$',ins.op_str)
            if m2:
                nm=sym(int(m2.group(1),16))
                if nm: t+='   ; <- '+nm
        for m2 in re.finditer(r'\[rip [+] 0x([0-9a-fA-F]+)\]',ins.op_str):
            nm=sym(ins.address+ins.size+int(m2.group(1),16))
            if nm: t+='   ; [rip] = %s'%nm; break
        print(t)
        if n+1>=maxn or ins.mnemonic=='ret': break

print('### 按名查地址')
for pat in sys.argv[1:]:
    for a,n in sorted([(a,n) for a,n in M.items() if pat.lower() in n.lower()]):
        print('  0x%x  %s'%(a,n))
print()
for pat in sys.argv[1:]:
    for a,n in sorted([(a,n) for a,n in M.items() if pat.lower() in n.lower()]):
        print('\n===== 0x%X %s ====='%(a,n))
        dis(a)
