namespace Acme.Workforce.Compensation.Tests.PayBand.Estimate;

using Acme.Workforce.Compensation.PayBand.Estimate;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PayBandMappingReportChecks
{
    public static void PayBandMappingReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709959280);
        var items = new List<PayBandMapping>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(PayBandMapping.NewPayBandMapping($"pay-band-mapping-{index}", "sample"));
        }
        var report = PayBandMappingReport.BuildPayBandMappingReport("test", items, clock);
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
        PayBandMappingReportCountsEachRecordOnce();
    }
}
