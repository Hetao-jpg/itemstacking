#!/usr/bin/env python
# -*- coding: utf-8 -*-
"""Alt 快移 · AltHandler.OnEventRelease 全量反汇编（直到 ret / 边界）"""
import json, re, sys, capstone, pefile

BIN    = r'D:/SteamLibrary/steamapps/common/Probably Stolen Demo\GameAssembly.dll'
SCRIPT = r'D:/games/Il2CppDumper/script.json'

pe = pefile.PE(BIN, fast_load=True)
IB = pe.OPTIONAL_HEADER.ImageBase
SECS = [(s.VirtualAddress, s.Misc_VirtualSize, s.PointerToRawData, s.SizeOfRawData) for s in pe.sections]
DATA = open(BIN, 'rb').read()

def rva2off(rva):
    for va, vsz, praw, sraw in SECS:
        if va <= rva < va + max(vsz, sraw):
            return praw + (rva - va)
    return None

d = json.load(open(SCRIPT, encoding='utf-8'))
M    = {e['Address']: e['Name'] for e in d.get('ScriptMethod', [])}
META = {e['Address']: e['Name'] for e in d.get('ScriptMetadata', [])}
MM   = {e['Address']: e['Name'] for e in d.get('ScriptMetadataMethod', [])}
STRS = {e['Address']: e['Value'] for e in d.get('ScriptString', [])}

def sym(va):
    for k in (va, va - IB):
        for t in (M, MM, META, STRS):
            if k in t:
                v = t[k]
                return '"%s"' % v[:80] if t is STRS else v
    return None

JMP = ('call','jmp','jne','je','ja','jb','jbe','jae','jg','jl','jge','jle','js','jns','jz','jnz')
md = capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_64); md.detail = True

def dis(rva, maxins=400):
    off = rva2off(rva)
    out = []
    for n, ins in enumerate(md.disasm(DATA[off:off+65536], IB + rva)):
        t = '0x%x:  %-9s %s' % (ins.address, ins.mnemonic, ins.op_str)
        if ins.mnemonic in JMP:
            m2 = re.search(r'0x([0-9a-fA-F]+)$', ins.op_str)
            if m2:
                nm = sym(int(m2.group(1), 16))
                if nm: t += '   ; <- ' + nm
        for m2 in re.finditer(r'\[rip [+] 0x([0-9a-fA-F]+)\]', ins.op_str):
            nm = sym(ins.address + ins.size + int(m2.group(1), 16))
            if nm:
                t += '   ; [rip+0x%x] = %s' % (int(m2.group(1), 16), nm); break
        out.append(t)
        if n + 1 >= maxins or ins.mnemonic == 'ret':
            break
    return out

name, rva = sys.argv[1], int(sys.argv[2], 16)
print('### %s RVA=0x%x' % (name, rva))
for line in dis(rva):
    print(line)
