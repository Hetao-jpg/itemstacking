// xUnit v2 默认按类分集合、集合之间并行 —— 而被测的三个方法全靠 Main 里的
// 静态集合 + 静态 Log 句柄（还有我们装的全局日志记录器），
// 并行跑会互相清空/抢写 ⇒ 偶发红。整个程序集串行执行。
[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]
