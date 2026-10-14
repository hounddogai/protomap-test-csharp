namespace Acme.Workforce.Contractors.Tests.Rate.Batch;

using Acme.Workforce.Contractors.Rate.Batch;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class RateNoticeReportChecks
{
    public static void RateNoticeReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701917017);
        var items = new List<RateNotice>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(RateNotice.NewRateNotice($"rate-notice-{index}", "sample"));
        }
        var report = RateNoticeReport.BuildRateNoticeReport("test", items, clock);
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
        RateNoticeReportCountsEachRecordOnce();
    }
}
