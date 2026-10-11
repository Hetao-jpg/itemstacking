#!/usr/bin/env python
# -*- coding: utf-8 -*-
"""DirectoryMaster.Item(0x49a010) 与 SlotMarker.TryAcceptOnce(0x7b0020) 反汇编"""
import json, re, capstone, pefile

BIN    = r'D:\SteamLibrary\steamapps\common\Probably Stolen Demo\GameAssembly.dll'
SCRIPT = r'D:\games\Il2CppDumper\script.json'

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
                return '"%s"' % v[:70] if t is STRS else v
    return None

md = capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_64); md.detail = True
JMP = ('call','jmp')

def dis(rva, end_rva=None, count=140, label=''):
    off = rva2off(rva)
    print('\n' + '=' * 96 + '\n### %s  0x%x\n' % (label, rva) + '=' * 96)
    n = 0
    for ins in md.disasm(DATA[off:off + 20000], IB + rva):
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
        print(t)
        n += 1
        if n >= count or ins.mnemonic == 'ret':
            break

dis(0x49a010, label='DirectoryMaster$$Item  (GetSplitItem 调用的工厂)')
dis(0x7b0020, label='SlotMarker$$TryAcceptOnce')
