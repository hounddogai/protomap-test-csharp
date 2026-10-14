namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationSchedule;

using Acme.Workforce.Compliance.Attestation.AttestationSchedule;

/// <summary>Checks of attestation schedule code, run by the project's runner.</summary>
public static class FallbackAttestationClaimViewChecks
{
    public static void FallbackAttestationClaimViewShowsTheStatusInCapitals()
    {
        var item = FallbackAttestationClaim.NewFallbackAttestationClaim("code-428", "label-351");
        var view = FallbackAttestationClaimView.ToFallbackAttestationClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackAttestationClaimView.ToFallbackAttestationClaimViews(new List<FallbackAttestationClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackAttestationClaimViewShowsTheStatusInCapitals();
    }
}
