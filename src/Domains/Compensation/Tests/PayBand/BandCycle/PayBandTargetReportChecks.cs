namespace Acme.Workforce.Compensation.Tests.PayBand.BandCycle;

using Acme.Workforce.Compensation.PayBand.BandCycle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of band cycle code, run by the project's runner.</summary>
public static class PayBandTargetReportChecks
{
    public static void PayBandTargetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702525836);
        var items = new List<PayBandTarget>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(PayBandTarget.NewPayBandTarget($"pay-band-target-{index}", "sample"));
        }
        var report = PayBandTargetReport.BuildPayBandTargetReport("test", items, clock);
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
        PayBandTargetReportCountsEachRecordOnce();
    }
}
