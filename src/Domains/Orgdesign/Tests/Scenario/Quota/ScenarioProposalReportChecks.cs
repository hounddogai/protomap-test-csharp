namespace Acme.Workforce.Orgdesign.Tests.Scenario.Quota;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Scenario.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class ScenarioProposalReportChecks
{
    public static void ScenarioProposalReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707712126);
        var items = new List<ScenarioProposal>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(ScenarioProposal.NewScenarioProposal($"scenario-proposal-{index}", "sample"));
        }
        var report = ScenarioProposalReport.BuildScenarioProposalReport("test", items, clock);
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
        ScenarioProposalReportCountsEachRecordOnce();
    }
}
