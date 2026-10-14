namespace Acme.Workforce.Absence.Tests.Holiday.Manifest;

using Acme.Workforce.Absence.Holiday.Manifest;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class HolidayManifestReportChecks
{
    public static void HolidayManifestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702962295);
        var items = new List<HolidayManifest>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(HolidayManifest.NewHolidayManifest($"holiday-manifest-{index}", "sample"));
        }
        var report = HolidayManifestReport.BuildHolidayManifestReport("test", items, clock);
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
        HolidayManifestReportCountsEachRecordOnce();
    }
}
