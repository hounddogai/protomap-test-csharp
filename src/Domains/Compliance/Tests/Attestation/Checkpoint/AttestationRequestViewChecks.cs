namespace Acme.Workforce.Compliance.Tests.Attestation.Checkpoint;

using Acme.Workforce.Compliance.Attestation.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class AttestationRequestViewChecks
{
    public static void AttestationRequestViewShowsTheStatusInCapitals()
    {
        var item = AttestationRequest.NewAttestationRequest("code-870", "label-709");
        var view = AttestationRequestView.ToAttestationRequestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AttestationRequestView.ToAttestationRequestViews(new List<AttestationRequest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AttestationRequestViewShowsTheStatusInCapitals();
    }
}
