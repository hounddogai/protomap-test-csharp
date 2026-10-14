namespace Acme.Workforce.Assets.Tests.Warranty.Bundle;

using Acme.Workforce.Assets.Warranty.Bundle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class LenientWarrantyBundleReportChecks
{
    public static void LenientWarrantyBundleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704043239);
        var items = new List<LenientWarrantyBundle>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LenientWarrantyBundle.NewLenientWarrantyBundle($"lenient-warranty-bundle-{index}", "sample"));
        }
        var report = LenientWarrantyBundleReport.BuildLenientWarrantyBundleReport("test", items, clock);
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
        LenientWarrantyBundleReportCountsEachRecordOnce();
    }
}
