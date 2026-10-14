using System;
using Il2Cpp;

namespace ItemStacking;

// IL2CPP 互操作安全取值 / 指针判据（全工程共用）
//
// ★ 这里的 catch 全部吞异常（返回 null/0/false）是刻意的防御设计，不是偷懒：
//   调用点多在 Harmony Prefix/Postfix 里，异常冒泡会打断原生输入管线 / 拖拽流程。
public partial class Main
{
    private static T TryGet<T>(Func<T> f) where T : class
    {
        try { return f(); } catch { return null; }
    }

    private static long Ptr(Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase o)
    {
        try { return o == null ? 0L : (long)o.Pointer; } catch { return 0L; }
    }

    // 容器的日志描述串："[id @指针]"（探针用）。
    private static string Describe(GameInventory inv)
    {
        if (inv == null) return "null";
        string id;
        try { id = inv.identifier; } catch { id = "?"; }
        return "[" + (string.IsNullOrEmpty(id) ? "<空id>" : id) + " @" + Ptr(inv).ToString("X") + "]";
    }

    // 安全取 identifier（对象可能已被销毁，属性读取会猝死）。
    private static string SafeId(object o)
    {
        try
        {
            if (o is GameItem gi) return gi.identifier;
            if (o is GameInventory inv) return inv.identifier;
            return o == null ? "null" : o.GetType().Name;
        }
        catch { return "?"; }
    }

    // 统一指针相等判据：两个 IL2CPP 包装对象是否指向同一 native 对象。
    //   ★ 用 Il2CppInterop 的 Pointer 比较，避免触发 GetHashCode/Equals 的托管往返。
    internal static bool SameNativePointer(object a, object b)
    {
        if (a == null || b == null) return false;
        if (ReferenceEquals(a, b)) return true;
        try
        {
            long pa = Ptr(a as Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase);
            long pb = Ptr(b as Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase);
            return pa != 0L && pa == pb;
        }
        catch { /* Pointer 读取失败 → 视为不同 */ }
        return false;
    }
}