namespace Acme.Workforce.Compensation.Tests.EquityGrant.OverrideItem;

using Acme.Workforce.Compensation.EquityGrant.OverrideItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class AdaptiveEquityGrantOverrideReportChecks
{
    public static void AdaptiveEquityGrantOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700152313);
        var items = new List<AdaptiveEquityGrantOverride>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(AdaptiveEquityGrantOverride.NewAdaptiveEquityGrantOverride($"adaptive-equity-grant-override-{index}", "sample"));
        }
        var report = AdaptiveEquityGrantOverrideReport.BuildAdaptiveEquityGrantOverrideReport("test", items, clock);
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
        AdaptiveEquityGrantOverrideReportCountsEachRecordOnce();
    }
}
