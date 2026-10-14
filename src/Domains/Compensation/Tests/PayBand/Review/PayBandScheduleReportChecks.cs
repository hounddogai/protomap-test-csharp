namespace Acme.Workforce.Compensation.Tests.PayBand.Review;

using Acme.Workforce.Compensation.PayBand.Review;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class PayBandScheduleReportChecks
{
    public static void PayBandScheduleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703507014);
        var items = new List<PayBandSchedule>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(PayBandSchedule.NewPayBandSchedule($"pay-band-schedule-{index}", "sample"));
        }
        var report = PayBandScheduleReport.BuildPayBandScheduleReport("test", items, clock);
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
        PayBandScheduleReportCountsEachRecordOnce();
    }
}
