namespace Acme.Workforce.Orgdesign.Tests.Scenario.Estimate;

using Acme.Workforce.Orgdesign.Scenario.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class ManualScenarioEstimateChecks
{
    public static void ManualScenarioEstimateStartsInRequested()
    {
        var item = ManualScenarioEstimate.NewManualScenarioEstimate("code-750", "name-890");
        Check.Equal(item.Status, ManualScenarioEstimateStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualScenarioEstimateMovesToRevoked()
    {
        var item = ManualScenarioEstimate.NewManualScenarioEstimate("code-750", "name-890");
        try
        {
            item.MoveTo(ManualScenarioEstimateStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualScenarioEstimateStatus.Revoked, "status");
    }

    public static void ManualScenarioEstimateRejectsMovesFromExpired()
    {
        var item = ManualScenarioEstimate.NewManualScenarioEstimate("code-750", "name-890");
        item.Status = ManualScenarioEstimateStatus.Expired;
        Check.Fails(() => item.MoveTo(ManualScenarioEstimateStatus.Requested), "move");
    }

    public static void RunAll()
    {
        ManualScenarioEstimateStartsInRequested();
        ManualScenarioEstimateMovesToRevoked();
        ManualScenarioEstimateRejectsMovesFromExpired();
    }
}
