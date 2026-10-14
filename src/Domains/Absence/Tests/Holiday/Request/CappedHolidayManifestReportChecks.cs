namespace Acme.Workforce.Absence.Tests.Holiday.Request;

using Acme.Workforce.Absence.Holiday.Request;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class CappedHolidayManifestReportChecks
{
    public static void CappedHolidayManifestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703197799);
        var items = new List<CappedHolidayManifest>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(CappedHolidayManifest.NewCappedHolidayManifest($"capped-holiday-manifest-{index}", "sample"));
        }
        var report = CappedHolidayManifestReport.BuildCappedHolidayManifestReport("test", items, clock);
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
        CappedHolidayManifestReportCountsEachRecordOnce();
    }
}
