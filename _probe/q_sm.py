p = r'D:/games/Il2CppDumper/dump.cs'
txt = open(p, encoding='utf-8', errors='ignore').read()
i = txt.find('public class SlotMarker')
seg = txt[i:i+3000]
for line in seg.split('\n')[:40]:
    if 'Fields' in line or ';' in line and '//' in line:
        print(line.strip()[:150])
