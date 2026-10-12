using System;
using Il2Cpp;
using MelonLoader;

namespace ItemStacking;

public partial class Main
{
    // ── 0.6.6：柜台容器 id 一次性打印 ──
    //   目的：让用户能拿到柜台容器的真实 identifier，以便填 ContainerWhitelist / ContainerBlacklist。
    //   柜台容器运行时创建，identifier 可能为空（这正是旧版 StackExclude="back_inv" 匹配不上的原因）。
    //   触发条件：EmporiumEntry.Instance 非空（进入交易场景）后打一次，每会话只打一次。
    //   无性能影响：一个 bool 判断 + 场景未进入时直接返回。
    //   （2026-10-11：原与 cfg 保活同处 CfgKeepAlive.cs；保活整体删除后，本探针独立保留。）
    private static bool _counterIdDumped;

    private static void CounterIdProbeTick()
    {
        if (!DiagOn) return;                            // 0.9.0：诊断总闸
        if (_counterIdDumped) return;
        try
        {
            var entry = EmporiumEntry.Instance;
            if (entry == null) return;                  // 未进交易场景 → 下帧再试
            _counterIdDumped = true;

            Log.Msg("[柜台id] 柜台容器详情（0.9.1 起含槽位/所属/内含指纹，用于 ContainerWhitelist 配置）：" +
                    "front="      + DescribeContainer(TryGet(() => entry.frontInvinvElement)) +
                    "\n[柜台id] back="    + DescribeContainer(TryGet(() => entry.backInvinvElement)) +
                    "\n[柜台id] counter=" + DescribeContainer(TryGet(() => entry.backInvinvElementCounter)) +
                    "\n[柜台id] 背包="     + DescribeContainer(TryGet(() => entry.invElement)) +
                    "\n[柜台id] 展示柜="   + DescribeContainer(TryGet(() => entry.showcaseElement)));
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            _counterIdDumped = true;   // 出错也别每帧重试
            LogOnce("counterid", Level.Warn, $"[柜台id] 输出失败: {e.Message}");
        }
    }
}
