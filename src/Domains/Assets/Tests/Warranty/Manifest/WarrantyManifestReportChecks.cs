namespace Acme.Workforce.Assets.Tests.Warranty.Manifest;

using Acme.Workforce.Assets.Warranty.Manifest;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class WarrantyManifestReportChecks
{
    public static void WarrantyManifestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707579422);
        var items = new List<WarrantyManifest>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(WarrantyManifest.NewWarrantyManifest($"warranty-manifest-{index}", "sample"));
        }
        var report = WarrantyManifestReport.BuildWarrantyManifestReport("test", items, clock);
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
        WarrantyManifestReportCountsEachRecordOnce();
    }
}
