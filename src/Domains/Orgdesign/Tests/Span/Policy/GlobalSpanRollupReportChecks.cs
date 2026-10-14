namespace Acme.Workforce.Orgdesign.Tests.Span.Policy;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Span.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class GlobalSpanRollupReportChecks
{
    public static void GlobalSpanRollupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700678676);
        var items = new List<GlobalSpanRollup>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(GlobalSpanRollup.NewGlobalSpanRollup($"global-span-rollup-{index}", "sample"));
        }
        var report = GlobalSpanRollupReport.BuildGlobalSpanRollupReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        GlobalSpanRollupReportCountsEachRecordOnce();
    }
}
