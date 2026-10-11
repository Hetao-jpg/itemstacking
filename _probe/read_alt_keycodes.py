import UnityPy, glob, os

data_dir = r"D:/SteamLibrary/steamapps/common/Probably Stolen Demo/Probably Stolen_Data"
cands = []
for pat in ("*.assets", "level*", "globalgamemanagers"):
    for p in glob.glob(os.path.join(data_dir, pat)):
        if os.path.isfile(p):
            cands.append(p)

for path in cands:
    try:
        env = UnityPy.load(path)
    except Exception:
        continue
    for obj in env.objects:
        if obj.type.name != "MonoBehaviour":
            continue
        try:
            raw = obj.get_raw_data()
        except Exception:
            continue
        # search for the class name in the raw bytes
        if b"ItemQuickTransferAltHandler" in raw or b"QuickTransfer" in raw:
            print(f"=== {os.path.basename(path)} obj {obj.path_id} len={len(raw)} ===")
            print(raw[:160].hex())
