namespace Acme.Workforce.Orgdesign.Tests.Reorg.Claim;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Reorg.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class ReorgRollupReportChecks
{
    public static void ReorgRollupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704827467);
        var items = new List<ReorgRollup>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(ReorgRollup.NewReorgRollup($"reorg-rollup-{index}", "sample"));
        }
        var report = ReorgRollupReport.BuildReorgRollupReport("test", items, clock);
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
        ReorgRollupReportCountsEachRecordOnce();
    }
}
