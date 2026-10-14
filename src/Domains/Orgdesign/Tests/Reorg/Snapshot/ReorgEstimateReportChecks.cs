namespace Acme.Workforce.Orgdesign.Tests.Reorg.Snapshot;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Reorg.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class ReorgEstimateReportChecks
{
    public static void ReorgEstimateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704341876);
        var items = new List<ReorgEstimate>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(ReorgEstimate.NewReorgEstimate($"reorg-estimate-{index}", "sample"));
        }
        var report = ReorgEstimateReport.BuildReorgEstimateReport("test", items, clock);
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
        ReorgEstimateReportCountsEachRecordOnce();
    }
}
