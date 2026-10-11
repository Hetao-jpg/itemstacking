#!/usr/bin/env python
# -*- coding: utf-8 -*-
"""反查字符串引用点：找哪个方法引用了给定字符串字面量。"""
import json, re, sys, bisect, capstone, pefile

BIN = r'D:\SteamLibrary\steamapps\common\Probably Stolen Demo\GameAssembly.dll'
SCRIPT = r'D:\games\Il2CppDumper\script.json'
SL = r'D:\games\Il2CppDumper\stringliteral.json'

pe = pefile.PE(BIN, fast_load=True)
IB = pe.OPTIONAL_HEADER.ImageBase
DATA = open(BIN, 'rb').read()
d = json.load(open(SCRIPT, encoding='utf-8'))
sl = json.load(open(SL, encoding='utf-8'))
M = {e['Address']: e['Name'] for e in d.get('ScriptMethod', [])}
MM = {e['Address']: e['Name'] for e in d.get('ScriptMetadataMethod', [])}
STARTS = sorted(M.keys())


def owner(va):
    i = bisect.bisect_right(STARTS, va) - 1
    return M[STARTS[i]] if i >= 0 else '?'


md = capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_64)

WANT = set(sys.argv[1:]) or {'inte_client_counter', 'front counter full', 'back counter full'}
targets = {}
for e in sl:
    if e['value'] in WANT:
        targets[int(e['address'], 16)] = e['value']

for s in pe.sections:
    nm = s.Name.rstrip(b'\x00')
    if nm not in (b'.text', b'il2cpp'):
        continue
    code = DATA[s.PointerToRawData:s.PointerToRawData + s.SizeOfRawData]
    base = IB + s.VirtualAddress
    for ins in md.disasm(code, base):
        m = re.search(r'\[rip [+] 0x([0-9a-fA-F]+)\]', ins.op_str)
        if not m:
            continue
        t = ins.address + ins.size + int(m.group(1), 16)
        for k in (t, t - IB):
            if k in targets:
                print('%-22s ref@ %x  in %s' % (targets[k], ins.address, owner(ins.address)))
                break
