namespace Acme.Workforce.Talent.Tests.CareerPath.Segment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.CareerPath.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class CareerPathPreferenceReportChecks
{
    public static void CareerPathPreferenceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708168325);
        var items = new List<CareerPathPreference>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(CareerPathPreference.NewCareerPathPreference($"career-path-preference-{index}", "sample"));
        }
        var report = CareerPathPreferenceReport.BuildCareerPathPreferenceReport("test", items, clock);
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
        CareerPathPreferenceReportCountsEachRecordOnce();
    }
}
