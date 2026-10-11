import json
p = r'D:/games/Il2CppDumper/script.json'
d = json.load(open(p, encoding='utf-8', errors='ignore'))
targets = {}
for m in d.get('ScriptMethod', []):
    n = m.get('Name','')
    if 'TryFindOneValidInventorySlot' in n:
        print(hex(m.get('Address',0)), n)
