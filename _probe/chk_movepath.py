#!/usr/bin/env python
# -*- coding: utf-8 -*-
# 检查 TryFindOneValidInventorySlot 是否查 MayStack（= 是否会被白名单影响）
import json, re, bisect, capstone, pefile
BIN = r'D:/SteamLibrary/steamapps/common/Probably Stolen Demo\GameAssembly.dll'
SCRIPT = r'D:/games/Il2CppDumper/script.json'
pe = pefile.PE(BIN, fast_load=True); IB = pe.OPTIONAL_HEADER.ImageBase
SECS = [(s.VirtualAddress, max(s.Misc_VirtualSize, s.SizeOfRawData), s.PointerToRawData) for s in pe.sections]
DATA = open(BIN,'rb').read()
def rva2off(rva):
    for va,sz,praw in SECS:
        if va <= rva < va+sz: return praw+(rva-va)
d = json.load(open(SCRIPT, encoding='utf-8'))
M  = {e['Address']: e['Name'] for e in d.get('ScriptMethod',[]) if e['Address']<0x10000000}
MM = {e['Address']: e['Name'] for e in d.get('ScriptMetadataMethod',[]) if e['Address']<0x10000000}
S=sorted(M.keys())
def nameat(va):
    return M.get(va) or MM.get(va)
for target in [0x7B0020, 0x766730, 0x77BB5C, 0x7844b0, 0x784580]:
    print('='*70)
    print('0x%x  %s' % (target, nameat(target)))
