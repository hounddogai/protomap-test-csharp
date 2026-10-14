namespace Acme.Workforce.Compensation.Tests.PayBand.Estimate;

using Acme.Workforce.Compensation.PayBand.Estimate;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class SeasonalPayBandQuotaReportChecks
{
    public static void SeasonalPayBandQuotaReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707861837);
        var items = new List<SeasonalPayBandQuota>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(SeasonalPayBandQuota.NewSeasonalPayBandQuota($"seasonal-pay-band-quota-{index}", "sample"));
        }
        var report = SeasonalPayBandQuotaReport.BuildSeasonalPayBandQuotaReport("test", items, clock);
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
        SeasonalPayBandQuotaReportCountsEachRecordOnce();
    }
}
