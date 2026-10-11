p = r'D:/games/Il2CppDumper/dump.cs'
txt = open(p, encoding='utf-8', errors='ignore').read()
i = txt.find('public abstract class GameInventory :')
seg = txt[i:i+40000]
import re
# find vtable-ish listing? dump.cs doesn't give vtable directly. Instead print methods order.
print(seg[:4000])
