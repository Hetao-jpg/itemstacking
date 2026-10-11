p = r'D:/games/Il2CppDumper/dump.cs'
txt = open(p, encoding='utf-8', errors='ignore').read()
i = txt.find('public abstract class GameInventory :')
seg = txt[i:i+40000]
for line in seg.split('\n'):
    if 'Slot:' in line or 'TryFindOneValid' in line:
        # print previous method line too
        print(line.strip()[:170])
