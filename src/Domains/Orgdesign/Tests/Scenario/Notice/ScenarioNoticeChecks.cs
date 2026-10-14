namespace Acme.Workforce.Orgdesign.Tests.Scenario.Notice;

using Acme.Workforce.Orgdesign.Scenario.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class ScenarioNoticeChecks
{
    public static void ScenarioNoticeStartsInOpen()
    {
        var item = ScenarioNotice.NewScenarioNotice("code-737", "title-244");
        Check.Equal(item.Status, ScenarioNoticeStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScenarioNoticeMovesToResolved()
    {
        var item = ScenarioNotice.NewScenarioNotice("code-737", "title-244");
        try
        {
            item.MoveTo(ScenarioNoticeStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScenarioNoticeStatus.Resolved, "status");
    }

    public static void ScenarioNoticeRejectsMovesFromArchived()
    {
        var item = ScenarioNotice.NewScenarioNotice("code-737", "title-244");
        item.Status = ScenarioNoticeStatus.Archived;
        Check.Fails(() => item.MoveTo(ScenarioNoticeStatus.Open), "move");
    }

    public static void RunAll()
    {
        ScenarioNoticeStartsInOpen();
        ScenarioNoticeMovesToResolved();
        ScenarioNoticeRejectsMovesFromArchived();
    }
}
