namespace Acme.Workforce.Compensation.Tests.PayBand.Reservation;

using Acme.Workforce.Compensation.PayBand.Reservation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class TieredPayBandEstimateReportChecks
{
    public static void TieredPayBandEstimateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708283659);
        var items = new List<TieredPayBandEstimate>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(TieredPayBandEstimate.NewTieredPayBandEstimate($"tiered-pay-band-estimate-{index}", "sample"));
        }
        var report = TieredPayBandEstimateReport.BuildTieredPayBandEstimateReport("test", items, clock);
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
        TieredPayBandEstimateReportCountsEachRecordOnce();
    }
}
