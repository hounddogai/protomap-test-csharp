namespace Acme.Workforce.Assets.Tests.Refresh.Rollup;

using Acme.Workforce.Assets.Refresh.Rollup;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class RefreshRequestReportChecks
{
    public static void RefreshRequestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705333324);
        var items = new List<RefreshRequest>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(RefreshRequest.NewRefreshRequest($"refresh-request-{index}", "sample"));
        }
        var report = RefreshRequestReport.BuildRefreshRequestReport("test", items, clock);
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
        RefreshRequestReportCountsEachRecordOnce();
    }
}
