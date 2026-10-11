#!/usr/bin/env python
# -*- coding: utf-8 -*-
exec(open('dis_tryaccept.py').read().split('# 找 GraphUtils')[0])
import json
d=json.load(open(SCRIPT,encoding='utf-8'))
# 找 back_inv_save_bag 的字符串地址
addr=None
for e in d.get('ScriptString',[]):
    if e['Value']=='back_inv_save_bag': addr=e['Address']
print('back_inv_save_bag string @ 0x%X'%addr if addr else 'not found')
# 全库找引用该字符串的方法（lea rip+）
targets=[]
if addr:
    for a in (addr, addr-IB):
        # 反查：扫描所有方法（太大）。改为扫描 .text 找 rip-relative 指向
        pass
# 直接扫描 .text 找 lea/mov [rip+x] 解析后 == addr
pe2=None
import pefile
p=pefile.PE(BIN, fast_load=True)
for s in p.sections:
    if b'.text' in s.Name:
        va,sz,praw=s.VirtualAddress,s.Misc_VirtualSize,s.PointerToRawData
code=DATA[praw:praw+sz]
found=[]
for ins in md.disasm(code, IB+va):
    m=re.search(r'\[rip [+] 0x([0-9a-fA-F]+)\]',ins.op_str)
    if not m: continue
    t=ins.address+ins.size+int(m.group(1),16)
    for k in (t, t-IB):
        if k==addr: found.append('0x%x: %s %s'%(ins.address-IB,ins.mnemonic,ins.op_str))
print('引用点 %d 个:'%len(found))
for f in found[:20]: print('  ',f)
