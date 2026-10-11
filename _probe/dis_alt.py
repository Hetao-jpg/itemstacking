#!/usr/bin/env python
# -*- coding: utf-8 -*-
"""ItemStacking 优化 · Alt 快移取证：GetSplitItem / TryAcceptAllMid / TryAcceptAll 反汇编"""
import json, re, sys, capstone, pefile

BIN    = r'D:\SteamLibrary\steamapps\common\Probably Stolen Demo\GameAssembly.dll'
SCRIPT = r'D:\games\Il2CppDumper\script.json'
DUMP   = r'D:\games\Il2CppDumper\dump.cs'

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
STARTS = sorted(M.keys())

def sym(va):
    for k in (va, va - IB):
        for t in (M, MM, META, STRS):
            if k in t:
                v = t[k]
                return '"%s"' % v[:70] if t is STRS else v
    return None

JMP = ('call','jmp','jne','je','ja','jb','jbe','jae','jg','jl','jge','jle','js','jns','jz','jnz')
md = capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_64); md.detail = True

def dis(rva, count=160):
    off = rva2off(rva)
    out = []
    for n, ins in enumerate(md.disasm(DATA[off:off+16384], IB + rva)):
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
        if n + 1 >= count or ins.mnemonic == 'ret':
            break
    return out

TARGETS = [
    ('GameItem.GetSplitItem',       0x797500),
    ('GraphUtils.TryAcceptAllMid',  0x7664B0),
    ('GraphUtils.TryAcceptAll',     0x766650),
    ('GraphUtils.TryAcceptOnce',    0x766730),
    ('GameItem.MaxNumRemove',       0x7984A0),
    ('GraphUtils.CanAccept',        0x7651C0),
]
if len(sys.argv) > 1:
    TARGETS = [(sys.argv[1], int(sys.argv[2], 16))]

for name, rva in TARGETS:
    print('\n' + '=' * 96)
    print('### %s   RVA=0x%x' % (name, rva))
    print('=' * 96)
    try:
        for line in dis(rva):
            print(line)
    except Exception as e:
        print('!! %s' % e)
