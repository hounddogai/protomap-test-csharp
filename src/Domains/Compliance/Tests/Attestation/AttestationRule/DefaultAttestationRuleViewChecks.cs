namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationRule;

using Acme.Workforce.Compliance.Attestation.AttestationRule;

/// <summary>Checks of attestation rule code, run by the project's runner.</summary>
public static class DefaultAttestationRuleViewChecks
{
    public static void DefaultAttestationRuleViewShowsTheStatusInCapitals()
    {
        var item = DefaultAttestationRule.NewDefaultAttestationRule("code-157", "name-78");
        var view = DefaultAttestationRuleView.ToDefaultAttestationRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultAttestationRuleView.ToDefaultAttestationRuleViews(new List<DefaultAttestationRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultAttestationRuleViewShowsTheStatusInCapitals();
    }
}
