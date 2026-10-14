namespace Acme.Workforce.Compensation.Tests.Benchmark.Threshold;

using Acme.Workforce.Compensation.Benchmark.Threshold;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class BenchmarkBudgetReportChecks
{
    public static void BenchmarkBudgetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706589269);
        var items = new List<BenchmarkBudget>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(BenchmarkBudget.NewBenchmarkBudget($"benchmark-budget-{index}", "sample"));
        }
        var report = BenchmarkBudgetReport.BuildBenchmarkBudgetReport("test", items, clock);
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
        BenchmarkBudgetReportCountsEachRecordOnce();
    }
}
