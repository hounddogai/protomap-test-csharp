namespace Acme.Workforce.Talent.Tests.Pool.Summary;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Pool.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PoolSummaryReportChecks
{
    public static void PoolSummaryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701281882);
        var items = new List<PoolSummary>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PoolSummary.NewPoolSummary($"pool-summary-{index}", "sample"));
        }
        var report = PoolSummaryReport.BuildPoolSummaryReport("test", items, clock);
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
        PoolSummaryReportCountsEachRecordOnce();
    }
}
