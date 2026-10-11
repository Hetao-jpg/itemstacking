#!/usr/bin/env python
# -*- coding: utf-8 -*-
"""在场景里找 EmporiumEntry，读其 backInvinvElementCounter 等容器对象的 identifier。"""
import UnityPy, os
GAME = r'D:/SteamLibrary/steamapps/common/Probably Stolen Demo\Probably Stolen_Data'
files = ['level0','level1','level2','resources.assets','sharedassets0.assets','sharedassets1.assets','sharedassets2.assets']
for f in files:
    p = os.path.join(GAME, f)
    if not os.path.exists(p): continue
    try: env = UnityPy.load(p)
    except Exception as e: continue
    found = False
    for obj in env.objects:
        if obj.type.name != 'MonoBehaviour': continue
        try: d = obj.read()
        except Exception: continue
        cls = ''
        try: cls = d.m_Script.read().m_ClassName
        except Exception:
            try: cls = d.m_Script.m_ClassName
            except Exception: pass
        if cls == 'EmporiumEntry':
            found = True
            print('FILE=%s CLS=%s pathid=%s' % (f, cls, obj.path_id))
            try:
                rd = obj.read_typetree()
                for k, v in rd.items():
                    if 'Invinv' in k or 'invElement' in k or 'Element' in k:
                        print('   %s = %s' % (k, v))
            except Exception as e:
                print('   err', e)
    if found: print('---')
print('done')
