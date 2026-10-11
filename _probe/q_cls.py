p = r'D:/games/Il2CppDumper/dump.cs'
txt = open(p, encoding='utf-8', errors='ignore').read()
import re
# find each "public class X : ... GameInventory" and print with TryFindOneValidInventorySlot context
for m in re.finditer(r'class (GameSlotInventory|GameGridInventory|GameGridScrollableInventory|GameInventory)\b', txt):
    st = m.start()
    # find the abstract/decl line
    seg = txt[st:st+300]
    print("---", seg.split('\n')[0][:120])
