namespace Acme.Workforce.Orgdesign.Tests.Scenario.Template;

using Acme.Workforce.Orgdesign.Scenario.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ScenarioLedgerChecks
{
    public static void ScenarioLedgerStartsInPlanned()
    {
        var item = ScenarioLedger.NewScenarioLedger("code-799", "title-328");
        Check.Equal(item.Status, ScenarioLedgerStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScenarioLedgerMovesToInProgress()
    {
        var item = ScenarioLedger.NewScenarioLedger("code-799", "title-328");
        try
        {
            item.MoveTo(ScenarioLedgerStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScenarioLedgerStatus.InProgress, "status");
    }

    public static void ScenarioLedgerRejectsMovesFromCancelled()
    {
        var item = ScenarioLedger.NewScenarioLedger("code-799", "title-328");
        item.Status = ScenarioLedgerStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ScenarioLedgerStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ScenarioLedgerStartsInPlanned();
        ScenarioLedgerMovesToInProgress();
        ScenarioLedgerRejectsMovesFromCancelled();
    }
}
