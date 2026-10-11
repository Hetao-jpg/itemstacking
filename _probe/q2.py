p = r'D:/games/Il2CppDumper/dump.cs'
txt = open(p, encoding='utf-8', errors='ignore').read()
import re
# find all occurrences of GameItem near "class"
for m in re.finditer(r'\bclass\s+GameItem\b', txt):
    print("=== match at", m.start())
    print(txt[m.start()-50:m.start()+300].replace('\n','\\n'))
    print()
