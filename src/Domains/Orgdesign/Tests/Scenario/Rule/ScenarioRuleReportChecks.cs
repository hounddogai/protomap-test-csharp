namespace Acme.Workforce.Orgdesign.Tests.Scenario.Rule;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Scenario.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class ScenarioRuleReportChecks
{
    public static void ScenarioRuleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700983928);
        var items = new List<ScenarioRule>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(ScenarioRule.NewScenarioRule($"scenario-rule-{index}", "sample"));
        }
        var report = ScenarioRuleReport.BuildScenarioRuleReport("test", items, clock);
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
        ScenarioRuleReportCountsEachRecordOnce();
    }
}
