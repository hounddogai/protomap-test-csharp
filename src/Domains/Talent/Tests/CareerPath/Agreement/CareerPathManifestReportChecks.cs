namespace Acme.Workforce.Talent.Tests.CareerPath.Agreement;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.CareerPath.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class CareerPathManifestReportChecks
{
    public static void CareerPathManifestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707377594);
        var items = new List<CareerPathManifest>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(CareerPathManifest.NewCareerPathManifest($"career-path-manifest-{index}", "sample"));
        }
        var report = CareerPathManifestReport.BuildCareerPathManifestReport("test", items, clock);
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
        CareerPathManifestReportCountsEachRecordOnce();
    }
}
