#!/usr/bin/env python
# -*- coding: utf-8 -*-
"""需求2 取证：找「Alt+左键点击把商品转移到柜台」的原版实现路径。

用户描述：原版 Alt+左键 = 转移商品到柜台（AddDirectSellingItemToTable / PlacedItemForSelling）。
本脚本：
  1) 反汇编 ItemMouseDragHandler.EndDrag（0x7BA010）看是否含「Alt → 柜台」分支
  2) 找所有 call PlayerStore.AddDirectSellingItemToTable / PlacedItemForSelling 的调用点
  3) 反汇编 PlayerStore.PlacedItemForSelling / AddDirectSellingItemToTableMiddle
"""
import json, re, sys, capstone, pefile

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

def method_end(rva):
    import bisect
    i = bisect.bisect_right(STARTS, rva)
    return STARTS[i] if i < len(STARTS) else rva + 0x1000

def dis(rva, end=None, maxn=600):
    off = rva2off(rva)
    if end is None:
        end = method_end(rva)
    out = []
    for n, ins in enumerate(md.disasm(DATA[off:off+(end-rva)], IB + rva)):
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
        if n + 1 >= maxn:
            break
    return out

# ── 1) 找所有 call AddDirectSellingItemToTable / PlacedItemForSelling 的站点 ──
def find_calls(target_rvas):
    """扫描整个 .text，找 call rel32 命中目标（同时比对 VA 与 RVA 两种表示）"""
    hits = []
    want = set()
    for r in target_rvas:
        want.add(r); want.add(IB + r); want.add(r - IB)
    for s in pe.sections:
        nm = s.Name.rstrip(b'\x00')
        if nm not in (b'.text', b'il2cpp'):
            continue
        base = s.PointerToRawData
        size = min(s.SizeOfRawData, len(DATA)-base)
        blob = DATA[base:base+size]
        for i in range(len(blob)-5):
            if blob[i] == 0xE8:  # call rel32
                rel = int.from_bytes(blob[i+1:i+5], 'little', signed=True)
                src = IB + s.VirtualAddress + i
                dst = src + 5 + rel
                if dst in want:
                    hits.append((src, dst))
    return hits

# ScriptMethod 里找目标
def find_method_addr(fullname_sub):
    for a, n in M.items():
        if fullname_sub in n:
            yield a, n

targets = {}
for pat in ['PlayerStore$$AddDirectSellingItemToTable', 'PlayerStore$$PlacedItemForSelling',
            'PlayerStore$$PlacedItemForBuying', 'PlayerStore$$AddedItemToClientArea']:
    for a, n in find_method_addr(pat):
        targets[a] = n
        print('TARGET %-55s RVA=0x%X' % (n, a))

print('\n=== call sites ===')
for dst in list(targets.keys()):
    for src, d2 in find_calls({dst}):
        print('call from 0x%X  -> %s' % (src, targets[dst]))
        nm = sym(src - 0x20) or sym(src)
        # 找最近的已知方法起点
        import bisect
        i = bisect.bisect_right(STARTS, src) - 1
        if i >= 0:
            print('        (in %s + 0x%X)' % (M.get(STARTS[i], '?'), src - STARTS[i]))

if len(sys.argv) > 1 and sys.argv[1] == 'dis':
    print('\n=== EndDrag ===')
    for l in dis(0x7BA010, 0x7BB750):
        print(l)
