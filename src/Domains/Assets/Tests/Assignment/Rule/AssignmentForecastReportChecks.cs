namespace Acme.Workforce.Assets.Tests.Assignment.Rule;

using Acme.Workforce.Assets.Assignment.Rule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class AssignmentForecastReportChecks
{
    public static void AssignmentForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704391217);
        var items = new List<AssignmentForecast>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(AssignmentForecast.NewAssignmentForecast($"assignment-forecast-{index}", "sample"));
        }
        var report = AssignmentForecastReport.BuildAssignmentForecastReport("test", items, clock);
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
        AssignmentForecastReportCountsEachRecordOnce();
    }
}
