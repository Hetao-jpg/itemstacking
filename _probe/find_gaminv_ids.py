#!/usr/bin/env python
# -*- coding: utf-8 -*-
"""列出场景里所有 GameInventory 派生对象的 identifier（含 counter 相关）。"""
import UnityPy, os
GAME = r'D:/SteamLibrary/steamapps/common/Probably Stolen Demo\Probably Stolen_Data'
WANT = ('GameInventory','GameGridInventory','GameSlotInventory','GameInventoryFunc')
files = sorted([x for x in os.listdir(GAME) if x.endswith('.assets') or x.startswith('level')])
seen=set()
for f in files:
    p = os.path.join(GAME, f)
    if not os.path.isfile(p): continue
    try: env = UnityPy.load(p)
    except Exception: continue
    for obj in env.objects:
        if obj.type.name != 'MonoBehaviour': continue
        try: d = obj.read()
        except Exception: continue
        cls=''
        try: cls = d.m_Script.read().m_ClassName
        except Exception:
            try: cls = d.m_Script.m_ClassName
            except Exception: pass
        if cls in WANT:
            try: rd = obj.read_typetree()
            except Exception: continue
            ident = rd.get('identifier')
            nm = rd.get('m_Name')
            key = (cls, ident)
            if key in seen: continue
            seen.add(key)
            print('%-20s identifier=%-32s name=%s  [%s]' % (cls, repr(ident), repr(nm), f))
print('done, total', len(seen))
