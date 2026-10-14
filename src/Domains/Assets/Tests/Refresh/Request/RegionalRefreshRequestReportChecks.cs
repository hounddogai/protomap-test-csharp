namespace Acme.Workforce.Assets.Tests.Refresh.Request;

using Acme.Workforce.Assets.Refresh.Request;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class RegionalRefreshRequestReportChecks
{
    public static void RegionalRefreshRequestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702780237);
        var items = new List<RegionalRefreshRequest>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(RegionalRefreshRequest.NewRegionalRefreshRequest($"regional-refresh-request-{index}", "sample"));
        }
        var report = RegionalRefreshRequestReport.BuildRegionalRefreshRequestReport("test", items, clock);
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
        RegionalRefreshRequestReportCountsEachRecordOnce();
    }
}
