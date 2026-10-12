using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

// 诊断 3：构造测试夹具所需的精确签名（BootstrapLibrary ctor / LogMsgFn 委托 / Library 属性）
class P
{
    static int Main(string[] args)
    {
        string ml = args.Length > 0 ? args[0]
            : @"D:\SteamLibrary\steamapps\common\Probably Stolen Demo\MelonLoader\net6\MelonLoader.dll";

        var rp = new ReaderParameters { ReadSymbols = false };
        rp.AssemblyResolver = new DirResolver(new[] { System.IO.Path.GetDirectoryName(ml) });
        var asm = AssemblyDefinition.ReadAssembly(ml, rp);
        var types = AllTypes(asm).ToList();

        var bl = types.First(x => x.FullName == "MelonLoader.InternalUtils.BootstrapLibrary");
        Console.WriteLine("== BootstrapLibrary ctors ==");
        foreach (var c in bl.Methods.Where(m => m.IsConstructor))
            Console.WriteLine("   ctor(" + (c.IsPublic ? "public" : c.IsAssembly ? "internal" : c.IsFamily ? "protected" : "private") +
                              ") (" + string.Join(", ", c.Parameters.Select(q => q.ParameterType.FullName + " " + q.Name)) + ")");

        Console.WriteLine("== BootstrapLibrary members of interest ==");
        foreach (var p in bl.Properties.Where(p => p.Name.Contains("LogMsg") || p.Name.Contains("LogError") || p.Name.Contains("GetLoaderConfig")))
            Console.WriteLine("   prop " + p.PropertyType.FullName + " " + p.Name +
                              " get=" + (p.GetMethod != null ? (p.GetMethod.IsPublic ? "public" : p.GetMethod.IsAssembly ? "internal" : "nonpub") : "-") +
                              " set=" + (p.SetMethod != null ? (p.SetMethod.IsPublic ? "public" : p.SetMethod.IsAssembly ? "internal" : "nonpub") : "-"));

        Console.WriteLine("== LogMsgFn delegate ==");
        foreach (var dname in new[]{"MelonLoader.Bootstrap.LogMsgFn","MelonLoader.Bootstrap.LogErrorFn","MelonLoader.Bootstrap.LogMelonInfoFn"}) { var fn = types.First(x => x.FullName == dname);
        var inv = fn.Methods.First(m => m.Name == "Invoke");
            Console.WriteLine(dname + " (" + string.Join(", ", inv.Parameters.Select(q => q.ParameterType.FullName + " " + q.Name)) + ") -> " + inv.ReturnType.FullName); }

        var ei = types.First(x => x.FullName == "MelonLoader.InternalUtils.BootstrapInterop");
        var lib = ei.Properties.First(p => p.Name == "Library");
        Console.WriteLine("== BootstrapInterop.Library ==");
        Console.WriteLine("   type=" + lib.PropertyType.FullName +
                          " get=" + (lib.GetMethod != null ? (lib.GetMethod.IsPublic ? "public" : lib.GetMethod.IsAssembly ? "internal" : "nonpub") : "-") +
                          " set=" + (lib.SetMethod != null ? (lib.SetMethod.IsPublic ? "public" : lib.SetMethod.IsAssembly ? "internal" : "nonpub") : "-"));
        var bf = bl.Fields.First(f => f.Name.Contains("LogMsg"));
        Console.WriteLine("   BootstrapLibrary backing field for LogMsg: " + bf.Name + " : " + bf.FieldType.FullName + " public=" + bf.IsPublic + " asm=" + bf.IsAssembly);

        Console.WriteLine("== also: does PassLogMsg dereference more than Library? ==");
        var pl = types.First(x => x.FullName == "MelonLoader.MelonLogger").Methods.First(m => m.Name == "PassLogMsg");
        foreach (var i in pl.Body.Instructions)
        {
            string s;
            if (i.Operand is MethodReference mr) s = i.OpCode + " " + mr.DeclaringType.FullName + "." + mr.Name;
            else if (i.Operand is FieldReference fr) s = i.OpCode + " " + fr.DeclaringType.FullName + "." + fr.Name;
            else s = i.OpCode + " " + i.Operand;
            Console.WriteLine("   " + s);
        }
        return 0;
    }

    class DirResolver : DefaultAssemblyResolver
    {
        public DirResolver(string[] dirs)
        {
            foreach (var d in dirs)
                if (!string.IsNullOrEmpty(d) && System.IO.Directory.Exists(d))
                    AddSearchDirectory(d);
        }
    }

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
