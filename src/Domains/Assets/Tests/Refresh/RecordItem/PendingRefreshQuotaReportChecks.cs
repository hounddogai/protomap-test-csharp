namespace Acme.Workforce.Assets.Tests.Refresh.RecordItem;

using Acme.Workforce.Assets.Refresh.RecordItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class PendingRefreshQuotaReportChecks
{
    public static void PendingRefreshQuotaReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708553098);
        var items = new List<PendingRefreshQuota>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PendingRefreshQuota.NewPendingRefreshQuota($"pending-refresh-quota-{index}", "sample"));
        }
        var report = PendingRefreshQuotaReport.BuildPendingRefreshQuotaReport("test", items, clock);
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
        PendingRefreshQuotaReportCountsEachRecordOnce();
    }
}
