#!/usr/bin/env python
# -*- coding: utf-8 -*-
"""DirectoryMaster.Item 尾部 + GeneralHelper.SetItemOwned + 谁调用 DirectoryMaster.Item"""
import json, re, capstone, pefile

BIN    = r'D:\SteamLibrary\steamapps\common\Probably Stolen Demo\GameAssembly.dll'
SCRIPT = r'D:\games\Il2CppDumper\script.json'
pe = pefile.PE(BIN, fast_load=True); IB = pe.OPTIONAL_HEADER.ImageBase
SECS = [(s.VirtualAddress, s.Misc_VirtualSize, s.PointerToRawData, s.SizeOfRawData) for s in pe.sections]
DATA = open(BIN, 'rb').read()
def rva2off(rva):
    for va, vsz, praw, sraw in SECS:
        if va <= rva < va + max(vsz, sraw): return praw + (rva - va)
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
                v = t[k]; return '"%s"' % v[:70] if t is STRS else v
    return None
md = capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_64); md.detail = True

def dis_range(a, b, label):
    off = rva2off(a)
    print('\n' + '=' * 96 + '\n### %s  0x%x - 0x%x\n' % (label, a, b) + '=' * 96)
    for ins in md.disasm(DATA[off:off + (b - a)], IB + a):
        t = '0x%x:  %-9s %s' % (ins.address, ins.mnemonic, ins.op_str)
        if ins.mnemonic in ('call', 'jmp'):
            m2 = re.search(r'0x([0-9a-fA-F]+)$', ins.op_str)
            if m2:
                nm = sym(int(m2.group(1), 16))
                if nm: t += '   ; <- ' + nm
        for m2 in re.finditer(r'\[rip [+] 0x([0-9a-fA-F]+)\]', ins.op_str):
            nm = sym(ins.address + ins.size + int(m2.group(1), 16))
            if nm: t += '   ; [rip+0x%x] = %s' % (int(m2.group(1), 16), nm); break
        print(t)

dis_range(0x49a2b0, 0x49a340, 'DirectoryMaster.Item 尾部')
dis_range(0x8b8220, 0x8b8360, 'GeneralHelper.SetItemOwned')

print('\n=== 谁调用 DirectoryMaster.Item (0x49a010) ===')
tgt = IB + 0x49a010; n = 0
for idx, st in enumerate(STARTS):
    end = STARTS[idx + 1] if idx + 1 < len(STARTS) else st + 0x4000
    o0 = rva2off(st)
    if o0 is None: continue
    try:
        for ins in md.disasm(DATA[o0:o0 + (end - st)], IB + st):
            if ins.address - IB >= end: break
            if ins.mnemonic == 'call':
                m2 = re.search(r'0x([0-9a-fA-F]+)$', ins.op_str)
                if m2 and int(m2.group(1), 16) == tgt:
                    print('  caller 0x%x  %s' % (ins.address - IB, M.get(st, '?'))); n += 1
    except Exception: continue
print('  共 %d 处' % n)
