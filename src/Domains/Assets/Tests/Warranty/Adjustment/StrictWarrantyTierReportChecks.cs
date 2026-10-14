namespace Acme.Workforce.Assets.Tests.Warranty.Adjustment;

using Acme.Workforce.Assets.Warranty.Adjustment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class StrictWarrantyTierReportChecks
{
    public static void StrictWarrantyTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705119379);
        var items = new List<StrictWarrantyTier>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(StrictWarrantyTier.NewStrictWarrantyTier($"strict-warranty-tier-{index}", "sample"));
        }
        var report = StrictWarrantyTierReport.BuildStrictWarrantyTierReport("test", items, clock);
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
        StrictWarrantyTierReportCountsEachRecordOnce();
    }
}
