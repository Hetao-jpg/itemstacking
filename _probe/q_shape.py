import re
p = r'D:/games/Il2CppDumper/dump.cs'
txt = open(p, encoding='utf-8', errors='ignore').read()
i = txt.find('public class GameItem :')
seg = txt[i:i+30000]
# print all lines containing shape/state/tag
for line in seg.split('\n'):
    l = line.strip()
    if 'hape' in l or 'State' in l or 'Grid' in l:
        print(l[:200])
