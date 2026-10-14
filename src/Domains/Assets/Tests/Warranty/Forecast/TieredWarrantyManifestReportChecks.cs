namespace Acme.Workforce.Assets.Tests.Warranty.Forecast;

using Acme.Workforce.Assets.Warranty.Forecast;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class TieredWarrantyManifestReportChecks
{
    public static void TieredWarrantyManifestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700785612);
        var items = new List<TieredWarrantyManifest>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(TieredWarrantyManifest.NewTieredWarrantyManifest($"tiered-warranty-manifest-{index}", "sample"));
        }
        var report = TieredWarrantyManifestReport.BuildTieredWarrantyManifestReport("test", items, clock);
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
        TieredWarrantyManifestReportCountsEachRecordOnce();
    }
}
