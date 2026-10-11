import sys
sys.path.insert(0, r"C:/Users/msi/.workbuddy/binaries/python/envs/default/Lib/site-packages")
import pefile
from capstone import *

GAME = r"D:/SteamLibrary/steamapps/common/Probably Stolen Demo/GameAssembly.dll"
pe = pefile.PE(GAME, fast_load=True)
base = pe.OPTIONAL_HEADER.ImageBase
data = pe.get_memory_mapped_image()
md = Cs(CS_ARCH_X86, CS_MODE_64)

def dis(va, n=80, name=""):
    off = va - base
    print(f"\n=== {name} @ {hex(va)} (off {hex(off)}) ===")
    blob = bytes(data[off:off+16*n])
    cnt=0
    for ins in md.disasm(blob, va):
        print(f"  {ins.address:#x}: {ins.mnemonic}\t{ins.op_str}")
        cnt+=1
        if cnt>=n: break
        if ins.mnemonic in ("ret","jmp"):
            break

# dump.cs RVA 是 "RVA" 列（相对 image base 0x180000000 的偏移？）
# dump 里 Offset 0x783180, VA 0x180784580 => RVA=0x784580, VA=base+0x784580
dis(base+0x784580, 50, "TryInventorySlot 3-arg")
dis(base+0x7844B0, 50, "TryInventorySlot 4-arg(reqnum)")
