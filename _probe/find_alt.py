#!/usr/bin/env python
# -*- coding: utf-8 -*-
"""全库扫描 Input.GetKey(LeftAlt=308 / 0x134) 的调用点，定位原版 Alt 相关路径。"""
import json, re, bisect, pefile
from collections import Counter

BIN    = r'D:\SteamLibrary\steamapps\common\Probably Stolen Demo\GameAssembly.dll'
SCRIPT = r'D:\games\Il2CppDumper\script.json'

pe = pefile.PE(BIN, fast_load=True)
IB = pe.OPTIONAL_HEADER.ImageBase
DATA = open(BIN, 'rb').read()

d = json.load(open(SCRIPT, encoding='utf-8'))
M = {e['Address']: e['Name'] for e in d.get('ScriptMethod', []) if e['Address'] < 0x10000000}
S = sorted(M.keys())

def enclosing(rva):
    i = bisect.bisect_right(S, rva) - 1
    return M[S[i]] if i >= 0 else '?'

# find Input.GetKey RVA
getkey = [a for a, n in M.items() if n.endswith('UnityEngine.Input$$GetKey')]
print('Input.GetKey RVAs:', [hex(x) for x in getkey])
want = set()
for g in getkey:
    want.add(g); want.add(IB + g)

# scan .text for call rel32 -> GetKey, then check preceding mov ecx, imm
text = None
sections = []
for s in pe.sections:
    nm = s.Name.rstrip(b'\x00')
    if nm in (b'.text', b'il2cpp'):
        sections.append(s)
hits = []
for text in sections:
    base = text.PointerToRawData
    blob = DATA[base:base+text.SizeOfRawData]
    for i in range(len(blob) - 5):
        if blob[i] == 0xE8:
            rel = int.from_bytes(blob[i+1:i+5], 'little', signed=True)
            src = IB + text.VirtualAddress + i
            dst = src + 5 + rel
            if dst in want:
                lo = max(0, i-12)
                ctx = blob[lo:i]
                m = None
                for k in range(len(ctx)-5, -1, -1):
                    if ctx[k] == 0xB9:
                        imm = int.from_bytes(ctx[k+1:k+5], 'little')
                        if imm < 0x400:
                            m = imm; break
                hits.append((src, m))

print('\ntotal GetKey calls:', len(hits))
c = Counter()
for src, key in hits:
    if key in (0x134, 0x130, 0x132, 0x131):  # Alt, Shift, Ctrl combos
        c[(key, enclosing(src - IB))] += 1
for (key, fn), n in c.most_common(80):
    print('key=0x%X %-70s x%d' % (key, fn, n))
