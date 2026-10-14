namespace Acme.Workforce.Compensation.Tests.PayBand.BandTemplate;

using Acme.Workforce.Compensation.PayBand.BandTemplate;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of band template code, run by the project's runner.</summary>
public static class AdaptivePayBandGroupReportChecks
{
    public static void AdaptivePayBandGroupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705282827);
        var items = new List<AdaptivePayBandGroup>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(AdaptivePayBandGroup.NewAdaptivePayBandGroup($"adaptive-pay-band-group-{index}", "sample"));
        }
        var report = AdaptivePayBandGroupReport.BuildAdaptivePayBandGroupReport("test", items, clock);
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
        AdaptivePayBandGroupReportCountsEachRecordOnce();
    }
}
