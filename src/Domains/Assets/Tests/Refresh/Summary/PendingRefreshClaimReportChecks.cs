namespace Acme.Workforce.Assets.Tests.Refresh.Summary;

using Acme.Workforce.Assets.Refresh.Summary;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PendingRefreshClaimReportChecks
{
    public static void PendingRefreshClaimReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707422860);
        var items = new List<PendingRefreshClaim>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PendingRefreshClaim.NewPendingRefreshClaim($"pending-refresh-claim-{index}", "sample"));
        }
        var report = PendingRefreshClaimReport.BuildPendingRefreshClaimReport("test", items, clock);
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
        PendingRefreshClaimReportCountsEachRecordOnce();
    }
}
