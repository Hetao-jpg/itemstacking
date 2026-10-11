p = r'D:/games/Il2CppDumper/dump.cs'
txt = open(p, encoding='utf-8', errors='ignore').read()
i = txt.find('public abstract class GameInventory : GameInventoryFunc, GraphNodeStorage')
j = txt.find('public class GameSlotInventory :', i)
seg = txt[i:j]
print("GameInventory body length:", len(seg))
# find TryFindOneValidInventorySlot in this precise segment
import re
for m in re.finditer(r'([^\n]*)\n\s*(public [^\n]*TryFindOneValidInventorySlot[^\n]*)', seg):
    print(">>>", m.group(1).strip()[:120])
    print("   ", m.group(2).strip()[:160])
