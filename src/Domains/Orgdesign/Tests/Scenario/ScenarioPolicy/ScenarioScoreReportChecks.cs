namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioPolicy;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

/// <summary>Checks of scenario policy code, run by the project's runner.</summary>
public static class ScenarioScoreReportChecks
{
    public static void ScenarioScoreReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708268626);
        var items = new List<ScenarioScore>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(ScenarioScore.NewScenarioScore($"scenario-score-{index}", "sample"));
        }
        var report = ScenarioScoreReport.BuildScenarioScoreReport("test", items, clock);
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
        ScenarioScoreReportCountsEachRecordOnce();
    }
}
