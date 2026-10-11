import json, re
# script.json maps RVA -> name
p = r'D:/games/Il2CppDumper/script.json'
txt = open(p, encoding='utf-8', errors='ignore').read()
# find name for Method$...RVA 0x7b01f0 and 0x751ea0 and 0x7856d0
import json
d = json.loads(txt)
# ScriptMethod list
for m in d.get('ScriptMethod', []):
    a = m.get('Address', 0)
    if a in (0x7B01F0, 0x751EA0, 0x7A0230, 0x7B0020):
        print(hex(a), m.get('Name'))
