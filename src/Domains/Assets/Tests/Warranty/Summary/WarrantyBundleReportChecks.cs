namespace Acme.Workforce.Assets.Tests.Warranty.Summary;

using Acme.Workforce.Assets.Warranty.Summary;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class WarrantyBundleReportChecks
{
    public static void WarrantyBundleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700011637);
        var items = new List<WarrantyBundle>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(WarrantyBundle.NewWarrantyBundle($"warranty-bundle-{index}", "sample"));
        }
        var report = WarrantyBundleReport.BuildWarrantyBundleReport("test", items, clock);
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
        WarrantyBundleReportCountsEachRecordOnce();
    }
}
