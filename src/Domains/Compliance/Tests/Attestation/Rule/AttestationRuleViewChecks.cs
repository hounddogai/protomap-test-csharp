namespace Acme.Workforce.Compliance.Tests.Attestation.Rule;

using Acme.Workforce.Compliance.Attestation.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class AttestationRuleViewChecks
{
    public static void AttestationRuleViewShowsTheStatusInCapitals()
    {
        var item = AttestationRule.NewAttestationRule("code-179", "title-420");
        var view = AttestationRuleView.ToAttestationRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AttestationRuleView.ToAttestationRuleViews(new List<AttestationRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AttestationRuleViewShowsTheStatusInCapitals();
    }
}
