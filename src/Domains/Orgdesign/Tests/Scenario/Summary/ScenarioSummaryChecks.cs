namespace Acme.Workforce.Orgdesign.Tests.Scenario.Summary;

using Acme.Workforce.Orgdesign.Scenario.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class ScenarioSummaryChecks
{
    public static void ScenarioSummaryStartsInOpen()
    {
        var item = ScenarioSummary.NewScenarioSummary("code-357", "title-208");
        Check.Equal(item.Status, ScenarioSummaryStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScenarioSummaryMovesToReopened()
    {
        var item = ScenarioSummary.NewScenarioSummary("code-357", "title-208");
        try
        {
            item.MoveTo(ScenarioSummaryStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScenarioSummaryStatus.Reopened, "status");
    }

    public static void ScenarioSummaryRejectsMovesFromArchived()
    {
        var item = ScenarioSummary.NewScenarioSummary("code-357", "title-208");
        item.Status = ScenarioSummaryStatus.Archived;
        Check.Fails(() => item.MoveTo(ScenarioSummaryStatus.Open), "move");
    }

    public static void RunAll()
    {
        ScenarioSummaryStartsInOpen();
        ScenarioSummaryMovesToReopened();
        ScenarioSummaryRejectsMovesFromArchived();
    }
}
