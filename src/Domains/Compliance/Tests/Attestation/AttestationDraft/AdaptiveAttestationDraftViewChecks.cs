namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationDraft;

using Acme.Workforce.Compliance.Attestation.AttestationDraft;

/// <summary>Checks of attestation draft code, run by the project's runner.</summary>
public static class AdaptiveAttestationDraftViewChecks
{
    public static void AdaptiveAttestationDraftViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveAttestationDraft.NewAdaptiveAttestationDraft("code-277", "name-660");
        var view = AdaptiveAttestationDraftView.ToAdaptiveAttestationDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveAttestationDraftView.ToAdaptiveAttestationDraftViews(new List<AdaptiveAttestationDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveAttestationDraftViewShowsTheStatusInCapitals();
    }
}
