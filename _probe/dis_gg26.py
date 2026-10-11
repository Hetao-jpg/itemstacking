import sys
sys.path.insert(0, r"C:/Users/msi/.workbuddy/binaries/python/envs/default/Lib/site-packages")
import pefile
from capstone import *

GAME = r"D:/SteamLibrary/steamapps/common/Probably Stolen Demo/GameAssembly.dll"
pe = pefile.PE(GAME, fast_load=True)
base = pe.OPTIONAL_HEADER.ImageBase
data = pe.get_memory_mapped_image()
md = Cs(CS_ARCH_X86, CS_MODE_64)

def dis(rva, n=200, name=""):
    va = base + rva
    print(f"\n=== {name} @ {hex(va)} ===")
    blob = bytes(data[rva:rva+16*n])
    cnt=0
    for ins in md.disasm(blob, va):
        print(f"  {ins.address:#x}: {ins.mnemonic}\t{ins.op_str}")
        cnt+=1
        if cnt>=n: break

# Note dump.cs says GameGridInventory.TryFindOneValidInventorySlot Slot26 @ 0x763AE0 (the real one we patch)
dis(0x763AE0, 160, "GameGridInventory.TryFindOneValidInventorySlot slot26")
