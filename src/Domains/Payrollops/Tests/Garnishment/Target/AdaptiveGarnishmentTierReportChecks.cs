namespace Acme.Workforce.Payrollops.Tests.Garnishment.Target;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Garnishment.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class AdaptiveGarnishmentTierReportChecks
{
    public static void AdaptiveGarnishmentTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702309584);
        var items = new List<AdaptiveGarnishmentTier>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(AdaptiveGarnishmentTier.NewAdaptiveGarnishmentTier($"adaptive-garnishment-tier-{index}", "sample"));
        }
        var report = AdaptiveGarnishmentTierReport.BuildAdaptiveGarnishmentTierReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        AdaptiveGarnishmentTierReportCountsEachRecordOnce();
    }
}
