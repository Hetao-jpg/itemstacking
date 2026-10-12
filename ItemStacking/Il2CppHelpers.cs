using System;
using Il2Cpp;

namespace ItemStacking;

// ══════════════════════════════════════════════════════════════════════════
//  IL2CPP 互操作安全取值 / 指针判据（2026-10-12 从 StackGates.cs 抽出）
//
//  这几个成员是全工程共用的「碰游戏对象的最小安全层」：
//    · TryGet            —— 包一层 getter 调用，IL2CPP 对象已销毁时返回 null 而不是抛异常
//    · Ptr               —— 取 native 指针（失败返回 0）
//    · Describe          —— 容器的日志描述串（id @指针）
//    · SafeId            —— 取 identifier 用于日志（对象可能已销毁）
//    · SameNativePointer —— 全工程唯一的「两个包装对象是否同一 native 对象」判据
//
//  ★ 抽出理由：它们与「堆叠闸门」无关，却散落在 StackGates.cs / StackPatches.AltTransfer.cs
//    里；按职责成文件后，StackGates.cs 只剩判据与闸门本身。
//  ★ 这里的 catch 全部吞异常（返回 null/0/false）是**刻意的防御设计**，不是偷懒：
//    调用点多在 Harmony Prefix/Postfix 里，异常冒泡会打断原生输入管线 / 拖拽流程。
// ══════════════════════════════════════════════════════════════════════════
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

    // 容器的日志描述串："[id @指针]"（探针/诊断输出用，原 StackGates.cs）。
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

    // 统一指针相等判据：两个 IL2CPP 包装对象是否指向同一 native 对象（含继承链 —— 柜台窗口下的子容器也算柜台）。
    //   ★ 用 Il2CppInterop 的 Pointer 比较，避免触发 GetHashCode/Equals 的托管往返。
    //   ★ 本方法即全工程唯一实现（原 AltPairTransfer.SameNativePtr 已并入此，消除重复判据）。
    //   ★ 2026-10-12：原 StackGates.SameInventory 只是本方法的一层语义包装，调用点已全部
    //     直接改调本方法（少一层跳转、少一个可被误改的判据入口）。
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
