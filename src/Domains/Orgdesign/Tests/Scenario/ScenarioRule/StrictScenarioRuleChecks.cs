namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioRule;

using Acme.Workforce.Orgdesign.Scenario.ScenarioRule;

/// <summary>Checks of scenario rule code, run by the project's runner.</summary>
public static class StrictScenarioRuleChecks
{
    public static void StrictScenarioRuleStartsInProposed()
    {
        var item = StrictScenarioRule.NewStrictScenarioRule("code-997", "label-909");
        Check.Equal(item.Status, StrictScenarioRuleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictScenarioRuleMovesToApproved()
    {
        var item = StrictScenarioRule.NewStrictScenarioRule("code-997", "label-909");
        try
        {
            item.MoveTo(StrictScenarioRuleStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictScenarioRuleStatus.Approved, "status");
    }

    public static void StrictScenarioRuleRejectsMovesFromWithdrawn()
    {
        var item = StrictScenarioRule.NewStrictScenarioRule("code-997", "label-909");
        item.Status = StrictScenarioRuleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(StrictScenarioRuleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        StrictScenarioRuleStartsInProposed();
        StrictScenarioRuleMovesToApproved();
        StrictScenarioRuleRejectsMovesFromWithdrawn();
    }
}
