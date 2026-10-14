namespace Acme.Workforce.Absence.Tests.Policy.PolicyRule;

using Acme.Workforce.Absence.Policy.PolicyRule;

/// <summary>Checks of policy rule code, run by the project's runner.</summary>
public static class TieredPolicyRuleChecks
{
    public static void TieredPolicyRuleStartsInCreated()
    {
        var item = TieredPolicyRule.NewTieredPolicyRule("code-596", "name-506");
        Check.Equal(item.Status, TieredPolicyRuleStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredPolicyRuleMovesToPublished()
    {
        var item = TieredPolicyRule.NewTieredPolicyRule("code-596", "name-506");
        try
        {
            item.MoveTo(TieredPolicyRuleStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredPolicyRuleStatus.Published, "status");
    }

    public static void TieredPolicyRuleRejectsMovesFromRetired()
    {
        var item = TieredPolicyRule.NewTieredPolicyRule("code-596", "name-506");
        item.Status = TieredPolicyRuleStatus.Retired;
        Check.Fails(() => item.MoveTo(TieredPolicyRuleStatus.Created), "move");
    }

    public static void RunAll()
    {
        TieredPolicyRuleStartsInCreated();
        TieredPolicyRuleMovesToPublished();
        TieredPolicyRuleRejectsMovesFromRetired();
    }
}
