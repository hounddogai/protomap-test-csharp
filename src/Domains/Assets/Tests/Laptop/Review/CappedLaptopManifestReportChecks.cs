namespace Acme.Workforce.Assets.Tests.Laptop.Review;

using Acme.Workforce.Assets.Laptop.Review;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class CappedLaptopManifestReportChecks
{
    public static void CappedLaptopManifestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707132039);
        var items = new List<CappedLaptopManifest>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(CappedLaptopManifest.NewCappedLaptopManifest($"capped-laptop-manifest-{index}", "sample"));
        }
        var report = CappedLaptopManifestReport.BuildCappedLaptopManifestReport("test", items, clock);
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
        CappedLaptopManifestReportCountsEachRecordOnce();
    }
}
