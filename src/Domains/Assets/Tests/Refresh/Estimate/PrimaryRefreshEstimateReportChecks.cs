namespace Acme.Workforce.Assets.Tests.Refresh.Estimate;

using Acme.Workforce.Assets.Refresh.Estimate;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PrimaryRefreshEstimateReportChecks
{
    public static void PrimaryRefreshEstimateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701342289);
        var items = new List<PrimaryRefreshEstimate>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PrimaryRefreshEstimate.NewPrimaryRefreshEstimate($"primary-refresh-estimate-{index}", "sample"));
        }
        var report = PrimaryRefreshEstimateReport.BuildPrimaryRefreshEstimateReport("test", items, clock);
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
        PrimaryRefreshEstimateReportCountsEachRecordOnce();
    }
}
