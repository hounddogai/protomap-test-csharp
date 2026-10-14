namespace Acme.Workforce.Workplace.Tests.Visitor.Draft;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Visitor.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class VisitorManifestReportChecks
{
    public static void VisitorManifestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702110652);
        var items = new List<VisitorManifest>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(VisitorManifest.NewVisitorManifest($"visitor-manifest-{index}", "sample"));
        }
        var report = VisitorManifestReport.BuildVisitorManifestReport("test", items, clock);
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
        VisitorManifestReportCountsEachRecordOnce();
    }
}
