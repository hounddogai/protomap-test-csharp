namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgPlan;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

/// <summary>Checks of reorg plan code, run by the project's runner.</summary>
public static class ManualReorgVarianceReportChecks
{
    public static void ManualReorgVarianceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703316357);
        var items = new List<ManualReorgVariance>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(ManualReorgVariance.NewManualReorgVariance($"manual-reorg-variance-{index}", "sample"));
        }
        var report = ManualReorgVarianceReport.BuildManualReorgVarianceReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        ManualReorgVarianceReportCountsEachRecordOnce();
    }
}
