import json
p = r'D:/games/Il2CppDumper/script.json'
d = json.load(open(p, encoding='utf-8', errors='ignore'))
targets = {0x751EA0, 0x7856D0, 0x77B750, 0x784580}
for m in d.get('ScriptMethod', []):
    a = m.get('Address', 0)
    if a in targets:
        print(hex(a), m.get('Name'))
