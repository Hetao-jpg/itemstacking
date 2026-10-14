using System;
using Il2Cpp;
using MelonLoader;

namespace ItemStacking;

// 柜台容器 id 一次性打印（配 ContainerWhitelist 用）
//
// ★ 目的：让用户能拿到柜台容器的真实 identifier / 槽位 / 所属链 / 内容指纹，
//   以便填 ContainerWhitelist / ContainerBlacklist。
// ★ 触发条件：EmporiumEntry.Instance 非空（进入交易场景）后打一次，每会话只打一次。
public partial class Main
{
    private static bool _counterIdDumped;

    private static void CounterIdProbeTick()
    {
        if (!DiagOn) return;
        if (_counterIdDumped) return;
        try
        {
            var entry = EmporiumEntry.Instance;
            if (entry == null) return;                  // 未进交易场景 → 下帧再试
            _counterIdDumped = true;

            Log.Msg("[柜台id] 柜台容器详情（用于 ContainerWhitelist 配置）：" +
                    "front=" + DescribeContainer(TryGet(() => entry.frontInvinvElement)) +
                    "\n[柜台id] back=" + DescribeContainer(TryGet(() => entry.backInvinvElement)) +
                    "\n[柜台id] counter=" + DescribeContainer(TryGet(() => entry.backInvinvElementCounter)) +
                    "\n[柜台id] 背包=" + DescribeContainer(TryGet(() => entry.invElement)) +
                    "\n[柜台id] 展示柜=" + DescribeContainer(TryGet(() => entry.showcaseElement)));
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            _counterIdDumped = true;   // 出错也别每帧重试
            LogOnce("counterid", Level.Warn, $"[柜台id] 输出失败: {e.Message}");
        }
    }
}