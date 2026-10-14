namespace Acme.Workforce.Talent.Tests.CareerPath.Slot;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.CareerPath.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class CareerPathOverrideReportChecks
{
    public static void CareerPathOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702807914);
        var items = new List<CareerPathOverride>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(CareerPathOverride.NewCareerPathOverride($"career-path-override-{index}", "sample"));
        }
        var report = CareerPathOverrideReport.BuildCareerPathOverrideReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        CareerPathOverrideReportCountsEachRecordOnce();
    }
}
