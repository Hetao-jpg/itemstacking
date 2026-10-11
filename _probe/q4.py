p = r'D:/games/Il2CppDumper/dump.cs'
txt = open(p, encoding='utf-8', errors='ignore').read()
i = txt.find('public abstract class GameInventory :')
seg = txt[i:i+80000]
import re
for line in seg.split('\n'):
    l = line.strip()
    if 'TryInventorySlot' in l or 'TryFindOneValidInventorySlot' in l or 'FindEmpty' in l.lower():
        print(l[:200])
