namespace Acme.Workforce.Absence.Tests.Policy.Rule;

using Acme.Workforce.Absence.Policy.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class PolicyRuleChecks
{
    public static void PolicyRuleStartsInNew()
    {
        var item = PolicyRule.NewPolicyRule("code-765", "name-162");
        Check.Equal(item.Status, PolicyRuleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyRuleMovesToQueued()
    {
        var item = PolicyRule.NewPolicyRule("code-765", "name-162");
        try
        {
            item.MoveTo(PolicyRuleStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyRuleStatus.Queued, "status");
    }

    public static void PolicyRuleRejectsMovesFromFailed()
    {
        var item = PolicyRule.NewPolicyRule("code-765", "name-162");
        item.Status = PolicyRuleStatus.Failed;
        Check.Fails(() => item.MoveTo(PolicyRuleStatus.New), "move");
    }

    public static void RunAll()
    {
        PolicyRuleStartsInNew();
        PolicyRuleMovesToQueued();
        PolicyRuleRejectsMovesFromFailed();
    }
}
