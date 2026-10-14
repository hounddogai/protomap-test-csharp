namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationEstimate;

using Acme.Workforce.Compliance.Attestation.AttestationEstimate;

/// <summary>Checks of attestation estimate code, run by the project's runner.</summary>
public static class FallbackAttestationSummaryViewChecks
{
    public static void FallbackAttestationSummaryViewShowsTheStatusInCapitals()
    {
        var item = FallbackAttestationSummary.NewFallbackAttestationSummary("code-742", "name-486");
        var view = FallbackAttestationSummaryView.ToFallbackAttestationSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackAttestationSummaryView.ToFallbackAttestationSummaryViews(new List<FallbackAttestationSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackAttestationSummaryViewShowsTheStatusInCapitals();
    }
}
