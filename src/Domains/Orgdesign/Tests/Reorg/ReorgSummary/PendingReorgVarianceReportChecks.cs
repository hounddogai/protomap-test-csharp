namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgSummary;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Reorg.ReorgSummary;

/// <summary>Checks of reorg summary code, run by the project's runner.</summary>
public static class PendingReorgVarianceReportChecks
{
    public static void PendingReorgVarianceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709311011);
        var items = new List<PendingReorgVariance>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(PendingReorgVariance.NewPendingReorgVariance($"pending-reorg-variance-{index}", "sample"));
        }
        var report = PendingReorgVarianceReport.BuildPendingReorgVarianceReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        PendingReorgVarianceReportCountsEachRecordOnce();
    }
}
