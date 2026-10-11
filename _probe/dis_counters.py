#!/usr/bin/env python
# -*- coding: utf-8 -*-
"""取证：EmporiumEntry 的柜台相关方法，判定哪个 grid = 玩家柜台 / 顾客柜台。
用法: python dis_counters.py 0x7ce010 [0x...]
"""
import json, re, sys, bisect, capstone, pefile

BIN = r'D:\SteamLibrary\steamapps\common\Probably Stolen Demo\GameAssembly.dll'
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
M = {e['Address']: e['Name'] for e in d.get('ScriptMethod', [])}
MM = {e['Address']: e['Name'] for e in d.get('ScriptMetadataMethod', [])}
STRS = {e['Address']: e['Value'] for e in d.get('ScriptString', [])}
STARTS = sorted(M.keys())


def sym(va):
    for k in (va, va - IB):
        for t in (M, MM, STRS):
            if k in t:
                v = t[k]
                return '"%s"' % v[:80] if t is STRS else v
    return None


md = capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_64)


def method_end(rva):
    i = bisect.bisect_right(STARTS, rva)
    return STARTS[i] if i < len(STARTS) else rva + 0x1500


def dis(rva, maxn=800):
    off = rva2off(rva)
    e = method_end(rva)
    print("\n===== 0x%x  len=%d =====" % (rva, e - rva))
    for n, ins in enumerate(md.disasm(DATA[off:off + (e - rva)], IB + rva)):
        t = '0x%x: %-8s %s' % (ins.address, ins.mnemonic, ins.op_str)
        m = re.search(r'0x([0-9a-fA-F]+)$', ins.op_str)
        if m and ins.mnemonic in ('call', 'jmp', 'je', 'jne', 'jz', 'jnz', 'jl', 'jg', 'jle', 'jge'):
            nm = sym(int(m.group(1), 16))
            if nm:
                t += '   ; <- ' + nm
        for m2 in re.finditer(r'\[rip [+] 0x([0-9a-fA-F]+)\]', ins.op_str):
            nm = sym(ins.address + ins.size + int(m2.group(1), 16))
            if nm:
                t += '   ; [rip] = %s' % nm
                break
        print(t)
        if n + 1 >= maxn:
            print('  ... (truncated at %d)' % maxn)
            break


if __name__ == '__main__':
    for a in sys.argv[1:]:
        dis(int(a, 16))
