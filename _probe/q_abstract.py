p = r'D:/games/Il2CppDumper/dump.cs'
txt = open(p, encoding='utf-8', errors='ignore').read()
import re
def find_decl(cls, meth):
    i = txt.find(cls)
    if i < 0: return "NO CLASS "+cls
    # class body until next "public class"/"public abstract class"
    m = re.search(r'\n(public (?:abstract |sealed )?class )', txt[i+10:])
    seg = txt[i:i+40000]
    for line in seg.split('\n'):
        if meth in line and ('public' in line or 'abstract' in line):
            return line.strip()[:170]
    return "not found"
for cls,meth in [("public abstract class GameItem : GameItemFunc","MayStack"),
                 ("public abstract class GameItem : GameItemFunc","GetStackSlot"),
                 ("public abstract class GameInventory ","TryFindOneValidInventorySlot")]:
    print(cls[:40], meth, "->", find_decl(cls, meth))
