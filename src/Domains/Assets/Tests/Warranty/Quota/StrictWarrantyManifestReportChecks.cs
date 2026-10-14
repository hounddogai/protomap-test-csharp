namespace Acme.Workforce.Assets.Tests.Warranty.Quota;

using Acme.Workforce.Assets.Warranty.Quota;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class StrictWarrantyManifestReportChecks
{
    public static void StrictWarrantyManifestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708300503);
        var items = new List<StrictWarrantyManifest>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(StrictWarrantyManifest.NewStrictWarrantyManifest($"strict-warranty-manifest-{index}", "sample"));
        }
        var report = StrictWarrantyManifestReport.BuildStrictWarrantyManifestReport("test", items, clock);
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
        StrictWarrantyManifestReportCountsEachRecordOnce();
    }
}
