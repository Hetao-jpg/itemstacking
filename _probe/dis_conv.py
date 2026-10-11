from capstone import *
import pefile
exe = r'D:/SteamLibrary/steamapps/common/Probably Stolen Demo\GameAssembly.dll'
pe = pefile.PE(exe, fast_load=True)
base = pe.OPTIONAL_HEADER.ImageBase
data = open(exe,'rb').read()
md = Cs(CS_ARCH_X86, CS_MODE_64); md.detail=True
def va2off(va): return pe.get_offset_from_rva(va - base)
def dis(va, ln, tag):
    off = va2off(va); code = data[off:off+ln]
    print(f"=== {tag} {hex(va)} ===")
    for ins in md.disasm(code, va):
        print(f"0x{ins.address:x}: {ins.mnemonic}\t{ins.op_str}")
# 3-arg convenience TryInventorySlot 0x784580
dis(base+0x784580, 130, "3-arg TryInventorySlot")
