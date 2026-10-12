#!/usr/bin/env python
# -*- coding: utf-8 -*-
"""定位纯度 tag 到底写 valueString 还是 internalValueString。
反汇编：TagState.SetString(0x7b21d0) / TagState.set_valueString(0x7b2770)
        / IngotPurityHelper.SetPurity(0x8d45a0)"""
import json, re, capstone, pefile

BIN    = r'D:\SteamLibrary\steamapps\common\Probably Stolen Demo\GameAssembly.dll'
SCRIPT = r'D:\工作区\反编译-原生\Il2CppDumper\script.json'

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
STRS = {e['Address']: e['Value'] for e in d.get('ScriptString', [])}
STARTS = sorted(M.keys())

def sym(va):
    for k in (va, va - IB):
        for t in (M, STRS):
            if k in t:
                v = t[k]
                return '"%s"' % v[:70] if t is STRS else v
    return None

md = capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_64); md.detail = True
JMP = ('call','jmp')

def dis(name, rva, length):
    off = rva2off(rva)
    print('=== %s (0x%x, %d bytes) ===' % (name, rva, length))
    for ins in md.disasm(DATA[off:off+length], IB + rva):
        t = '0x%x:  %-9s %s' % (ins.address, ins.mnemonic, ins.op_str)
        if ins.mnemonic in JMP:
            m2 = re.search(r'0x([0-9a-fA-F]+)$', ins.op_str)
            if m2:
                nm = sym(int(m2.group(1), 16))
                if nm: t += '   ; <- ' + nm
        for m2 in re.finditer(r'\[rip [+] 0x([0-9a-fA-F]+)\]', ins.op_str):
            nm = sym(ins.address + ins.size + int(m2.group(1), 16))
            if nm:
                t += '   ; [rip] = %s' % nm; break
        # 字段偏移访问 [reg+offset]
        for m2 in re.finditer(r'\[r[a-z0-9]+ \+ 0x([0-9a-fA-F]+)\]', ins.op_str):
            t += '   ; field@+0x%s' % m2.group(1)
        print(t)
    print()

# TagState.SetString
dis('TagState.SetString', 0x7b21d0, 0x60)
# TagState.set_valueString
dis('TagState.set_valueString', 0x7b2770, 0x80)
# IngotPurityHelper.SetPurity（看它调 SetString 还是别的）
dis('IngotPurityHelper.SetPurity', 0x8d45a0, 0x120)
