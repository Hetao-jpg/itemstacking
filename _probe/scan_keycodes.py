import UnityPy, glob, os, struct

data_dir = r"D:/SteamLibrary/steamapps/common/Probably Stolen Demo/Probably Stolen_Data"
files = []
for name in os.listdir(data_dir):
    p = os.path.join(data_dir, name)
    if os.path.isfile(p) and (name.endswith(".assets") or name.startswith("level")):
        files.append(p)

target_names = ("ItemQuickTransferAltHandler",)

for path in files:
    try:
        env = UnityPy.load(path)
    except Exception as e:
        continue
    for obj in env.objects:
        if obj.type.name != "MonoBehaviour":
            continue
        try:
            raw = obj.get_raw_data()
        except Exception:
            continue
        # MonoBehaviour: PPtr m_Script(12) then fields. Check script name via typetree on m_Script
        if len(raw) < 40:
            continue
        # look for ints 308 (LeftAlt), 323 (Mouse0), 306 (LeftControl), 324 (Mouse1) in first 48 bytes
        head = raw[:48]
        ints = struct.unpack("<" + "i"*9, head[:36])
        interesting = [v for v in ints if v in (304,305,306,307,308,309,310,311,312,313,323,324,325,326,327,328,329)]
        if len(interesting) >= 2:
            print(f"{os.path.basename(path)} obj={obj.path_id} len={len(raw)} ints={ints[:9]}")
