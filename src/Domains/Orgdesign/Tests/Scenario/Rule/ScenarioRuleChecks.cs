namespace Acme.Workforce.Orgdesign.Tests.Scenario.Rule;

using Acme.Workforce.Orgdesign.Scenario.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class ScenarioRuleChecks
{
    public static void ScenarioRuleStartsInDraft()
    {
        var item = ScenarioRule.NewScenarioRule("code-557", "label-384");
        Check.Equal(item.Status, ScenarioRuleStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScenarioRuleMovesToPending()
    {
        var item = ScenarioRule.NewScenarioRule("code-557", "label-384");
        try
        {
            item.MoveTo(ScenarioRuleStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScenarioRuleStatus.Pending, "status");
    }

    public static void ScenarioRuleRejectsMovesFromClosed()
    {
        var item = ScenarioRule.NewScenarioRule("code-557", "label-384");
        item.Status = ScenarioRuleStatus.Closed;
        Check.Fails(() => item.MoveTo(ScenarioRuleStatus.Draft), "move");
    }

    public static void RunAll()
    {
        ScenarioRuleStartsInDraft();
        ScenarioRuleMovesToPending();
        ScenarioRuleRejectsMovesFromClosed();
    }
}
