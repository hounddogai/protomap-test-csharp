namespace Acme.Workforce.Compensation.Tests.Benchmark.Rule;

using Acme.Workforce.Compensation.Benchmark.Rule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class BenchmarkRuleReportChecks
{
    public static void BenchmarkRuleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703911096);
        var items = new List<BenchmarkRule>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(BenchmarkRule.NewBenchmarkRule($"benchmark-rule-{index}", "sample"));
        }
        var report = BenchmarkRuleReport.BuildBenchmarkRuleReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        BenchmarkRuleReportCountsEachRecordOnce();
    }
}
