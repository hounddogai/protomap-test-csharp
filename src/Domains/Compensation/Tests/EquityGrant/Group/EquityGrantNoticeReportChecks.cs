namespace Acme.Workforce.Compensation.Tests.EquityGrant.Group;

using Acme.Workforce.Compensation.EquityGrant.Group;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class EquityGrantNoticeReportChecks
{
    public static void EquityGrantNoticeReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708992460);
        var items = new List<EquityGrantNotice>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(EquityGrantNotice.NewEquityGrantNotice($"equity-grant-notice-{index}", "sample"));
        }
        var report = EquityGrantNoticeReport.BuildEquityGrantNoticeReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        EquityGrantNoticeReportCountsEachRecordOnce();
    }
}
