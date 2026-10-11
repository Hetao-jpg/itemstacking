using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

class P
{
    class DirResolver : DefaultAssemblyResolver
    {
        public DirResolver(string[] dirs)
        {
            foreach (var d in dirs)
                if (!string.IsNullOrEmpty(d) && System.IO.Directory.Exists(d))
                    AddSearchDirectory(d);
        }
    }

    static int Main(string[] args)
    {
        string asmPath = args.Length > 0 ? args[0]
            : @"D:\SteamLibrary\steamapps\common\Probably Stolen Demo\MelonLoader\Il2CppAssemblies\Assembly-CSharp.dll";

        var rp = new ReaderParameters { ReadSymbols = false };
        var gameDir = @"D:\SteamLibrary\steamapps\common\Probably Stolen Demo\MelonLoader\Il2CppAssemblies";
        var mlNet = @"D:\SteamLibrary\steamapps\common\Probably Stolen Demo\MelonLoader\net35";
        var mlNet6 = @"D:\SteamLibrary\steamapps\common\Probably Stolen Demo\MelonLoader\net6";
        rp.AssemblyResolver = new DirResolver(new[] { gameDir, mlNet, mlNet6, System.IO.Path.GetDirectoryName(asmPath) });
        var asm = AssemblyDefinition.ReadAssembly(asmPath, rp);

        TypeDefinition FindType(string full)
        {
            foreach (var t in asm.MainModule.Types)
            {
                var r = FindRec(t, full);
                if (r != null) return r;
            }
            return null;
        }
        TypeDefinition FindRec(TypeDefinition t, string full)
        {
            if (t.FullName == full) return t;
            foreach (var n in t.NestedTypes) { var r = FindRec(n, full); if (r != null) return r; }
            return null;
        }

        var baseInv = FindType("Il2Cpp.GameInventory");
        var gameItem = FindType("Il2Cpp.GameItem");
        Console.WriteLine($"GameInventory: {(baseInv!=null)} abstract={baseInv?.IsAbstract}");
        Console.WriteLine($"GameItem: {(gameItem!=null)}");

        // strict subclass: walk BaseType chain
        bool StrictSub(TypeDefinition t, TypeDefinition baseT)
        {
            for (var cur = t; cur != null; cur = cur.BaseType?.Resolve())
            {
                if (cur == baseT) return true;
                if (cur.FullName == "Il2CppSystem.Object" || cur.FullName == "System.Object") break;
                if (cur == t && cur.BaseType == null) break;
            }
            return false;
        }

        Console.WriteLine("\n-- all types named *Inventory or GameItem --");
        foreach (var t in AllTypes(asm))
        {
            if (!(t.Name.Contains("Inventory") || t.Name == "GameItem")) continue;
            Console.WriteLine($"  {t.FullName}  base={t.BaseType?.FullName}  abstract={t.IsAbstract}  strictSub(Inv)={baseInv!=null && StrictSub(t, baseInv)}");
        }

        Console.WriteLine("\n-- candidate methods on strict subclasses --");
        foreach (var t in AllTypes(asm))
        {
            if (t.IsAbstract || baseInv == null || !StrictSub(t, baseInv)) continue;
            foreach (var m in t.Methods)
            {
                if (m.Name != "TryFindOneValidInventorySlot") continue;
                if (m.Parameters.Count != 1) continue;
                if (m.Parameters[0].ParameterType.FullName != "Il2Cpp.GameItem") continue;
                bool isNewSlot = m.IsNewSlot && !m.IsVirtual ? false : m.IsNewSlot && m.IsVirtual && !IsOverride(m);
                Console.WriteLine($"  {t.FullName}.{m.Name}  virtual={m.IsVirtual} newSlot={m.IsNewSlot} final={m.IsFinal} override={IsOverride(m)}");
            }
        }
        return 0;
    }

    static bool IsOverride(MethodDefinition m)
        => m.IsVirtual && !m.IsNewSlot;

    static IEnumerable<TypeDefinition> AllTypes(AssemblyDefinition asm)
    {
        var stack = new Stack<TypeDefinition>(asm.MainModule.Types);
        while (stack.Count > 0)
        {
            var t = stack.Pop();
            yield return t;
            foreach (var n in t.NestedTypes) stack.Push(n);
        }
    }
}
