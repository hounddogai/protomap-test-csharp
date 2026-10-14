namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgProfile;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Reorg.ReorgProfile;

/// <summary>Checks of reorg profile code, run by the project's runner.</summary>
public static class ReorgAdjustmentReportChecks
{
    public static void ReorgAdjustmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704497989);
        var items = new List<ReorgAdjustment>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(ReorgAdjustment.NewReorgAdjustment($"reorg-adjustment-{index}", "sample"));
        }
        var report = ReorgAdjustmentReport.BuildReorgAdjustmentReport("test", items, clock);
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
        ReorgAdjustmentReportCountsEachRecordOnce();
    }
}
