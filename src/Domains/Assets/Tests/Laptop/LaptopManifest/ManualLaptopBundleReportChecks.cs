namespace Acme.Workforce.Assets.Tests.Laptop.LaptopManifest;

using Acme.Workforce.Assets.Laptop.LaptopManifest;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of laptop manifest code, run by the project's runner.</summary>
public static class ManualLaptopBundleReportChecks
{
    public static void ManualLaptopBundleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702186810);
        var items = new List<ManualLaptopBundle>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(ManualLaptopBundle.NewManualLaptopBundle($"manual-laptop-bundle-{index}", "sample"));
        }
        var report = ManualLaptopBundleReport.BuildManualLaptopBundleReport("test", items, clock);
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
        ManualLaptopBundleReportCountsEachRecordOnce();
    }
}
