namespace Acme.Workforce.Talent.Tests.CareerPath.Quota;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.CareerPath.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class CareerPathWindowReportChecks
{
    public static void CareerPathWindowReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702366728);
        var items = new List<CareerPathWindow>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(CareerPathWindow.NewCareerPathWindow($"career-path-window-{index}", "sample"));
        }
        var report = CareerPathWindowReport.BuildCareerPathWindowReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        CareerPathWindowReportCountsEachRecordOnce();
    }
}
