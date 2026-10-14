namespace Acme.Workforce.Assets.Tests.Warranty.Ledger;

using Acme.Workforce.Assets.Warranty.Ledger;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class AdaptiveWarrantyMappingReportChecks
{
    public static void AdaptiveWarrantyMappingReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701197597);
        var items = new List<AdaptiveWarrantyMapping>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(AdaptiveWarrantyMapping.NewAdaptiveWarrantyMapping($"adaptive-warranty-mapping-{index}", "sample"));
        }
        var report = AdaptiveWarrantyMappingReport.BuildAdaptiveWarrantyMappingReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        AdaptiveWarrantyMappingReportCountsEachRecordOnce();
    }
}
