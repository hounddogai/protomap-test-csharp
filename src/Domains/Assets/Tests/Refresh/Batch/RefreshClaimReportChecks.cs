namespace Acme.Workforce.Assets.Tests.Refresh.Batch;

using Acme.Workforce.Assets.Refresh.Batch;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class RefreshClaimReportChecks
{
    public static void RefreshClaimReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705499894);
        var items = new List<RefreshClaim>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(RefreshClaim.NewRefreshClaim($"refresh-claim-{index}", "sample"));
        }
        var report = RefreshClaimReport.BuildRefreshClaimReport("test", items, clock);
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
        RefreshClaimReportCountsEachRecordOnce();
    }
}
