namespace Acme.Workforce.Compliance.Tests.Attestation.Window;

using Acme.Workforce.Compliance.Attestation.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class AttestationCheckpointViewChecks
{
    public static void AttestationCheckpointViewShowsTheStatusInCapitals()
    {
        var item = AttestationCheckpoint.NewAttestationCheckpoint("code-122", "label-697");
        var view = AttestationCheckpointView.ToAttestationCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AttestationCheckpointView.ToAttestationCheckpointViews(new List<AttestationCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AttestationCheckpointViewShowsTheStatusInCapitals();
    }
}
