import sys
sys.path.insert(0, r"C:/Users/msi/.workbuddy/binaries/python/envs/default/Lib/site-packages")
import pefile
from capstone import *
GAME = r"D:/SteamLibrary/steamapps/common/Probably Stolen Demo/GameAssembly.dll"
pe = pefile.PE(GAME, fast_load=True)
base = pe.OPTIONAL_HEADER.ImageBase
data = pe.get_memory_mapped_image()
md = Cs(CS_ARCH_X86, CS_MODE_64)
def dis(rva, n=400, name="", start=0):
    va = base + rva + start
    print(f"\n=== {name} @ {hex(va)} ===")
    blob = bytes(data[rva+start:rva+start+16*n])
    cnt=0
    for ins in md.disasm(blob, va):
        print(f"  {ins.address:#x}: {ins.mnemonic}\t{ins.op_str}")
        cnt+=1
        if cnt>=n: break
dis(0x77B750, 260, "…持续", start=0x2B0)   # 从 0x77BA00 附近继续
