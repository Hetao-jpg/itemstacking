#!/usr/bin/env python
# -*- coding: utf-8 -*-
"""读游戏序列化数据中 ItemQuickTransfer* 的 key / quickTransferKey 实际配置值。"""
import UnityPy, os
GAME = r'D:\SteamLibrary\steamapps\common\Probably Stolen Demo\Probably Stolen_Data'
want = ('ItemQuickTransferHandler', 'ItemQuickTransferAltHandler', 'ItemQuickTransferAltHandler1')
files = ['level0', 'level1', 'globalgamemanagers.assets', 'resources.assets',
         'sharedassets0.assets', 'sharedassets1.assets']
for f in files:
    p = os.path.join(GAME, f)
    if not os.path.exists(p):
        continue
    try:
        env = UnityPy.load(p)
    except Exception as e:
        print('load fail', f, e); continue
    for obj in env.objects:
        if obj.type.name != 'MonoBehaviour':
            continue
        try:
            d = obj.read()
        except Exception:
            continue
        cls = ''
        try:
            cls = d.m_Script.read().m_ClassName
        except Exception:
            try: cls = d.m_Script.m_ClassName
            except Exception: pass
        if cls in want:
            print('FILE=%s CLS=%s' % (f, cls))
            try:
                rd = obj.read_typetree()
                for k, v in rd.items():
                    if k in ('key', 'quickTransferKey', 'isQuickTransfer', 'm_Name'):
                        print('   %s = %s' % (k, v))
            except Exception as e:
                print('   typetree err', e)
print('done')
