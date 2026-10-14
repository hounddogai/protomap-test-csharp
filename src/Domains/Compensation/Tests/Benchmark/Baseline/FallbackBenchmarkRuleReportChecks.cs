namespace Acme.Workforce.Compensation.Tests.Benchmark.Baseline;

using Acme.Workforce.Compensation.Benchmark.Baseline;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class FallbackBenchmarkRuleReportChecks
{
    public static void FallbackBenchmarkRuleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702291777);
        var items = new List<FallbackBenchmarkRule>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(FallbackBenchmarkRule.NewFallbackBenchmarkRule($"fallback-benchmark-rule-{index}", "sample"));
        }
        var report = FallbackBenchmarkRuleReport.BuildFallbackBenchmarkRuleReport("test", items, clock);
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
        FallbackBenchmarkRuleReportCountsEachRecordOnce();
    }
}
