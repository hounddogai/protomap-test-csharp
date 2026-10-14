namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioPolicy;

using Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

/// <summary>Checks of scenario policy code, run by the project's runner.</summary>
public static class ScenarioTierChecks
{
    public static void ScenarioTierStartsInProposed()
    {
        var item = ScenarioTier.NewScenarioTier("code-298", "name-825");
        Check.Equal(item.Status, ScenarioTierStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScenarioTierMovesToRejected()
    {
        var item = ScenarioTier.NewScenarioTier("code-298", "name-825");
        try
        {
            item.MoveTo(ScenarioTierStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScenarioTierStatus.Rejected, "status");
    }

    public static void ScenarioTierRejectsMovesFromWithdrawn()
    {
        var item = ScenarioTier.NewScenarioTier("code-298", "name-825");
        item.Status = ScenarioTierStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ScenarioTierStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ScenarioTierStartsInProposed();
        ScenarioTierMovesToRejected();
        ScenarioTierRejectsMovesFromWithdrawn();
    }
}
