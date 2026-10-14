namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgClaim;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Reorg.ReorgClaim;

/// <summary>Checks of reorg claim code, run by the project's runner.</summary>
public static class RollingReorgSummaryReportChecks
{
    public static void RollingReorgSummaryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703780324);
        var items = new List<RollingReorgSummary>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(RollingReorgSummary.NewRollingReorgSummary($"rolling-reorg-summary-{index}", "sample"));
        }
        var report = RollingReorgSummaryReport.BuildRollingReorgSummaryReport("test", items, clock);
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
        RollingReorgSummaryReportCountsEachRecordOnce();
    }
}
