namespace Acme.Workforce.Assets.Tests.Warranty.Allocation;

using Acme.Workforce.Assets.Warranty.Allocation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class ScheduledWarrantyManifestReportChecks
{
    public static void ScheduledWarrantyManifestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702831641);
        var items = new List<ScheduledWarrantyManifest>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(ScheduledWarrantyManifest.NewScheduledWarrantyManifest($"scheduled-warranty-manifest-{index}", "sample"));
        }
        var report = ScheduledWarrantyManifestReport.BuildScheduledWarrantyManifestReport("test", items, clock);
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
        ScheduledWarrantyManifestReportCountsEachRecordOnce();
    }
}
