namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioEstimate;

using Acme.Workforce.Orgdesign.Scenario.ScenarioEstimate;

/// <summary>Checks of scenario estimate code, run by the project's runner.</summary>
public static class CappedScenarioEstimateChecks
{
    public static void CappedScenarioEstimateStartsInPlanned()
    {
        var item = CappedScenarioEstimate.NewCappedScenarioEstimate("code-782", "title-369");
        Check.Equal(item.Status, CappedScenarioEstimateStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedScenarioEstimateMovesToScheduled()
    {
        var item = CappedScenarioEstimate.NewCappedScenarioEstimate("code-782", "title-369");
        try
        {
            item.MoveTo(CappedScenarioEstimateStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedScenarioEstimateStatus.Scheduled, "status");
    }

    public static void CappedScenarioEstimateRejectsMovesFromCancelled()
    {
        var item = CappedScenarioEstimate.NewCappedScenarioEstimate("code-782", "title-369");
        item.Status = CappedScenarioEstimateStatus.Cancelled;
        Check.Fails(() => item.MoveTo(CappedScenarioEstimateStatus.Planned), "move");
    }

    public static void RunAll()
    {
        CappedScenarioEstimateStartsInPlanned();
        CappedScenarioEstimateMovesToScheduled();
        CappedScenarioEstimateRejectsMovesFromCancelled();
    }
}
