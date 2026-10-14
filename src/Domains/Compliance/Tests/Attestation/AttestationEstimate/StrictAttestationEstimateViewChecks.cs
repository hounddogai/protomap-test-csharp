namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationEstimate;

using Acme.Workforce.Compliance.Attestation.AttestationEstimate;

/// <summary>Checks of attestation estimate code, run by the project's runner.</summary>
public static class StrictAttestationEstimateViewChecks
{
    public static void StrictAttestationEstimateViewShowsTheStatusInCapitals()
    {
        var item = StrictAttestationEstimate.NewStrictAttestationEstimate("code-219", "name-582");
        var view = StrictAttestationEstimateView.ToStrictAttestationEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictAttestationEstimateView.ToStrictAttestationEstimateViews(new List<StrictAttestationEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictAttestationEstimateViewShowsTheStatusInCapitals();
    }
}
