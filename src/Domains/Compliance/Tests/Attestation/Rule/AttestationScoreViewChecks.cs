namespace Acme.Workforce.Compliance.Tests.Attestation.Rule;

using Acme.Workforce.Compliance.Attestation.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class AttestationScoreViewChecks
{
    public static void AttestationScoreViewShowsTheStatusInCapitals()
    {
        var item = AttestationScore.NewAttestationScore("code-144", "title-234");
        var view = AttestationScoreView.ToAttestationScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AttestationScoreView.ToAttestationScoreViews(new List<AttestationScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AttestationScoreViewShowsTheStatusInCapitals();
    }
}
