namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioAudit;

using Acme.Workforce.Orgdesign.Scenario.ScenarioAudit;

/// <summary>Checks of scenario audit code, run by the project's runner.</summary>
public static class PrimaryScenarioTierChecks
{
    public static void PrimaryScenarioTierStartsInProposed()
    {
        var item = PrimaryScenarioTier.NewPrimaryScenarioTier("code-842", "title-255");
        Check.Equal(item.Status, PrimaryScenarioTierStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryScenarioTierMovesToApproved()
    {
        var item = PrimaryScenarioTier.NewPrimaryScenarioTier("code-842", "title-255");
        try
        {
            item.MoveTo(PrimaryScenarioTierStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryScenarioTierStatus.Approved, "status");
    }

    public static void PrimaryScenarioTierRejectsMovesFromWithdrawn()
    {
        var item = PrimaryScenarioTier.NewPrimaryScenarioTier("code-842", "title-255");
        item.Status = PrimaryScenarioTierStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PrimaryScenarioTierStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PrimaryScenarioTierStartsInProposed();
        PrimaryScenarioTierMovesToApproved();
        PrimaryScenarioTierRejectsMovesFromWithdrawn();
    }
}
