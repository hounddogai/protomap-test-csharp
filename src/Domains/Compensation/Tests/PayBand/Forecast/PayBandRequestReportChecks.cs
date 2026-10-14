namespace Acme.Workforce.Compensation.Tests.PayBand.Forecast;

using Acme.Workforce.Compensation.PayBand.Forecast;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class PayBandRequestReportChecks
{
    public static void PayBandRequestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701819421);
        var items = new List<PayBandRequest>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PayBandRequest.NewPayBandRequest($"pay-band-request-{index}", "sample"));
        }
        var report = PayBandRequestReport.BuildPayBandRequestReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        PayBandRequestReportCountsEachRecordOnce();
    }
}
