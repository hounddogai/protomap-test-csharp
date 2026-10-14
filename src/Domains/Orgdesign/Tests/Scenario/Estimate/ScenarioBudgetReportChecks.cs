namespace Acme.Workforce.Orgdesign.Tests.Scenario.Estimate;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Scenario.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class ScenarioBudgetReportChecks
{
    public static void ScenarioBudgetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704175444);
        var items = new List<ScenarioBudget>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(ScenarioBudget.NewScenarioBudget($"scenario-budget-{index}", "sample"));
        }
        var report = ScenarioBudgetReport.BuildScenarioBudgetReport("test", items, clock);
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
        ScenarioBudgetReportCountsEachRecordOnce();
    }
}
