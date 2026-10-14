namespace Acme.Workforce.Compliance.Tests.Attestation.Schedule;

using Acme.Workforce.Compliance.Attestation.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class DefaultAttestationClaimViewChecks
{
    public static void DefaultAttestationClaimViewShowsTheStatusInCapitals()
    {
        var item = DefaultAttestationClaim.NewDefaultAttestationClaim("code-884", "name-957");
        var view = DefaultAttestationClaimView.ToDefaultAttestationClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultAttestationClaimView.ToDefaultAttestationClaimViews(new List<DefaultAttestationClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultAttestationClaimViewShowsTheStatusInCapitals();
    }
}
