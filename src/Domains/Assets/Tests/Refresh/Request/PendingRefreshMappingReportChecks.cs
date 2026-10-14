namespace Acme.Workforce.Assets.Tests.Refresh.Request;

using Acme.Workforce.Assets.Refresh.Request;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class PendingRefreshMappingReportChecks
{
    public static void PendingRefreshMappingReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700147021);
        var items = new List<PendingRefreshMapping>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(PendingRefreshMapping.NewPendingRefreshMapping($"pending-refresh-mapping-{index}", "sample"));
        }
        var report = PendingRefreshMappingReport.BuildPendingRefreshMappingReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        PendingRefreshMappingReportCountsEachRecordOnce();
    }
}
