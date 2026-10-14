namespace Acme.Workforce.Absence.Tests.Policy.PolicyRule;

using Acme.Workforce.Absence.Policy.PolicyRule;

/// <summary>Checks of policy rule code, run by the project's runner.</summary>
public static class TieredPolicyRuleViewChecks
{
    public static void TieredPolicyRuleViewShowsTheStatusInCapitals()
    {
        var item = TieredPolicyRule.NewTieredPolicyRule("code-596", "name-506");
        var view = TieredPolicyRuleView.ToTieredPolicyRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredPolicyRuleView.ToTieredPolicyRuleViews(new List<TieredPolicyRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredPolicyRuleViewShowsTheStatusInCapitals();
    }
}
