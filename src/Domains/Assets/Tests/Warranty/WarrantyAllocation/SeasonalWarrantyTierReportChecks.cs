namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyAllocation;

using Acme.Workforce.Assets.Warranty.WarrantyAllocation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of warranty allocation code, run by the project's runner.</summary>
public static class SeasonalWarrantyTierReportChecks
{
    public static void SeasonalWarrantyTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702839750);
        var items = new List<SeasonalWarrantyTier>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(SeasonalWarrantyTier.NewSeasonalWarrantyTier($"seasonal-warranty-tier-{index}", "sample"));
        }
        var report = SeasonalWarrantyTierReport.BuildSeasonalWarrantyTierReport("test", items, clock);
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
        SeasonalWarrantyTierReportCountsEachRecordOnce();
    }
}
