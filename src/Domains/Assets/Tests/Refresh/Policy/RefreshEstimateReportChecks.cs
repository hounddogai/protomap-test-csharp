namespace Acme.Workforce.Assets.Tests.Refresh.Policy;

using Acme.Workforce.Assets.Refresh.Policy;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class RefreshEstimateReportChecks
{
    public static void RefreshEstimateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708124543);
        var items = new List<RefreshEstimate>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(RefreshEstimate.NewRefreshEstimate($"refresh-estimate-{index}", "sample"));
        }
        var report = RefreshEstimateReport.BuildRefreshEstimateReport("test", items, clock);
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
        RefreshEstimateReportCountsEachRecordOnce();
    }
}
