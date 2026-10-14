namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationDraft;

using Acme.Workforce.Compliance.Attestation.AttestationDraft;

/// <summary>Checks of attestation draft code, run by the project's runner.</summary>
public static class PendingAttestationThresholdViewChecks
{
    public static void PendingAttestationThresholdViewShowsTheStatusInCapitals()
    {
        var item = PendingAttestationThreshold.NewPendingAttestationThreshold("code-246", "label-184");
        var view = PendingAttestationThresholdView.ToPendingAttestationThresholdView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingAttestationThresholdView.ToPendingAttestationThresholdViews(new List<PendingAttestationThreshold> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingAttestationThresholdViewShowsTheStatusInCapitals();
    }
}
