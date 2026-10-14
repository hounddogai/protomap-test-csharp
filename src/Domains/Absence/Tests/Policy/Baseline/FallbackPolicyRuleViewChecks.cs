namespace Acme.Workforce.Absence.Tests.Policy.Baseline;

using Acme.Workforce.Absence.Policy.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class FallbackPolicyRuleViewChecks
{
    public static void FallbackPolicyRuleViewShowsTheStatusInCapitals()
    {
        var item = FallbackPolicyRule.NewFallbackPolicyRule("code-483", "title-489");
        var view = FallbackPolicyRuleView.ToFallbackPolicyRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackPolicyRuleView.ToFallbackPolicyRuleViews(new List<FallbackPolicyRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackPolicyRuleViewShowsTheStatusInCapitals();
    }
}
