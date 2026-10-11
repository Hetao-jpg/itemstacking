#!/usr/bin/env python
# -*- coding: utf-8 -*-
# 反编译 MelonLoader.dll 里 string 序列化相关方法
import subprocess, sys
dll = r'D:/SteamLibrary/steamapps/common/Probably Stolen Demo/MelonLoader/net6/MelonLoader.dll'
import os
if not os.path.exists(dll):
    for root,dirs,files in os.walk(r'D:/SteamLibrary/steamapps/common/Probably Stolen Demo/MelonLoader'):
        for f in files:
            if f=='MelonLoader.dll':
                dll=os.path.join(root,f); print('found:', dll)
print(subprocess.run([r'C:/Users/msi/.dotnet/tools/ilspycmd.exe','-l','c',dll],capture_output=True,text=True).stdout[:2000])
