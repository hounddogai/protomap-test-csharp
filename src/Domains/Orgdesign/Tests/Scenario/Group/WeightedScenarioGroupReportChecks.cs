namespace Acme.Workforce.Orgdesign.Tests.Scenario.Group;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Scenario.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class WeightedScenarioGroupReportChecks
{
    public static void WeightedScenarioGroupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707633640);
        var items = new List<WeightedScenarioGroup>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(WeightedScenarioGroup.NewWeightedScenarioGroup($"weighted-scenario-group-{index}", "sample"));
        }
        var report = WeightedScenarioGroupReport.BuildWeightedScenarioGroupReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        WeightedScenarioGroupReportCountsEachRecordOnce();
    }
}
