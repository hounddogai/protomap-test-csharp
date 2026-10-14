namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioTemplate;

using Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

/// <summary>Checks of scenario template code, run by the project's runner.</summary>
public static class CappedScenarioLedgerChecks
{
    public static void CappedScenarioLedgerStartsInIdle()
    {
        var item = CappedScenarioLedger.NewCappedScenarioLedger("code-346", "title-115");
        Check.Equal(item.Status, CappedScenarioLedgerStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedScenarioLedgerMovesToWarming()
    {
        var item = CappedScenarioLedger.NewCappedScenarioLedger("code-346", "title-115");
        try
        {
            item.MoveTo(CappedScenarioLedgerStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedScenarioLedgerStatus.Warming, "status");
    }

    public static void CappedScenarioLedgerRejectsMovesFromStopped()
    {
        var item = CappedScenarioLedger.NewCappedScenarioLedger("code-346", "title-115");
        item.Status = CappedScenarioLedgerStatus.Stopped;
        Check.Fails(() => item.MoveTo(CappedScenarioLedgerStatus.Idle), "move");
    }

    public static void RunAll()
    {
        CappedScenarioLedgerStartsInIdle();
        CappedScenarioLedgerMovesToWarming();
        CappedScenarioLedgerRejectsMovesFromStopped();
    }
}
