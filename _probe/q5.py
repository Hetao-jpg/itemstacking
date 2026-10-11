import subprocess, os
# search the interop assembly for GridShape property on GameItem
asm = r'D:/SteamLibrary/steamapps/common/Probably Stolen Demo\MelonLoader\Il2CppAssemblies\Assembly-CSharp.dll'
# use ilspycmd
tool = r'C:/Users/msi/.dotnet/tools/ilspycmd.exe'
out = subprocess.run([tool, '-t', 'GameItem', asm], capture_output=True, text=True, errors='ignore')
t = out.stdout
import re
for line in t.split('\n'):
    l = line.strip()
    if 'GridShape' in l and ('shape' in l.lower()):
        print(l[:200])
