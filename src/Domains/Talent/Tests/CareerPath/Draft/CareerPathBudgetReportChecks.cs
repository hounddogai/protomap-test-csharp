namespace Acme.Workforce.Talent.Tests.CareerPath.Draft;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.CareerPath.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class CareerPathBudgetReportChecks
{
    public static void CareerPathBudgetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706803674);
        var items = new List<CareerPathBudget>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(CareerPathBudget.NewCareerPathBudget($"career-path-budget-{index}", "sample"));
        }
        var report = CareerPathBudgetReport.BuildCareerPathBudgetReport("test", items, clock);
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
        CareerPathBudgetReportCountsEachRecordOnce();
    }
}
