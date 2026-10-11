p = r'D:/games/Il2CppDumper/dump.cs'
txt = open(p, encoding='utf-8', errors='ignore').read()
i = txt.find('public abstract class GameItem : GameItemFunc')
seg = txt[i:i+40000]
# print only fields + property methods
import re
for line in seg.split('\n'):
    l = line.strip()
    if 'BackingField' in l and '0x1' in l:
        print(l[:180])
print("---PROPS---")
for m in re.finditer(r'public ([^\n{};=]*?) (get_[A-Za-z0-9_]+)\(\)', seg):
    print(m.group(1).strip(), m.group(2))
