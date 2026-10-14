namespace Acme.Workforce.Compensation.Tests.PayBand.Tier;

using Acme.Workforce.Compensation.PayBand.Tier;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class LegacyPayBandRollupReportChecks
{
    public static void LegacyPayBandRollupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709488376);
        var items = new List<LegacyPayBandRollup>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(LegacyPayBandRollup.NewLegacyPayBandRollup($"legacy-pay-band-rollup-{index}", "sample"));
        }
        var report = LegacyPayBandRollupReport.BuildLegacyPayBandRollupReport("test", items, clock);
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
        LegacyPayBandRollupReportCountsEachRecordOnce();
    }
}
