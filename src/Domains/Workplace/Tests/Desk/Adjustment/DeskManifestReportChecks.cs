namespace Acme.Workforce.Workplace.Tests.Desk.Adjustment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Desk.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class DeskManifestReportChecks
{
    public static void DeskManifestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704312477);
        var items = new List<DeskManifest>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(DeskManifest.NewDeskManifest($"desk-manifest-{index}", "sample"));
        }
        var report = DeskManifestReport.BuildDeskManifestReport("test", items, clock);
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
        DeskManifestReportCountsEachRecordOnce();
    }
}
