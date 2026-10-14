namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationSchedule;

using Acme.Workforce.Compliance.Attestation.AttestationSchedule;

/// <summary>Checks of attestation schedule code, run by the project's runner.</summary>
public static class PrimaryAttestationScoreViewChecks
{
    public static void PrimaryAttestationScoreViewShowsTheStatusInCapitals()
    {
        var item = PrimaryAttestationScore.NewPrimaryAttestationScore("code-217", "name-892");
        var view = PrimaryAttestationScoreView.ToPrimaryAttestationScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryAttestationScoreView.ToPrimaryAttestationScoreViews(new List<PrimaryAttestationScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryAttestationScoreViewShowsTheStatusInCapitals();
    }
}
