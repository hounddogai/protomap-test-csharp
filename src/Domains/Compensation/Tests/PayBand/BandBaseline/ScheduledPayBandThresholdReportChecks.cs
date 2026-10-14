namespace Acme.Workforce.Compensation.Tests.PayBand.BandBaseline;

using Acme.Workforce.Compensation.PayBand.BandBaseline;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of band baseline code, run by the project's runner.</summary>
public static class ScheduledPayBandThresholdReportChecks
{
    public static void ScheduledPayBandThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706922387);
        var items = new List<ScheduledPayBandThreshold>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(ScheduledPayBandThreshold.NewScheduledPayBandThreshold($"scheduled-pay-band-threshold-{index}", "sample"));
        }
        var report = ScheduledPayBandThresholdReport.BuildScheduledPayBandThresholdReport("test", items, clock);
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
        ScheduledPayBandThresholdReportCountsEachRecordOnce();
    }
}
