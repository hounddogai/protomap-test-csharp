namespace Acme.Workforce.Compliance.Tests.Attestation.Window;

using Acme.Workforce.Compliance.Attestation.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class AttestationCheckpointChecks
{
    public static void AttestationCheckpointStartsInOpen()
    {
        var item = AttestationCheckpoint.NewAttestationCheckpoint("code-122", "label-697");
        Check.Equal(item.Status, AttestationCheckpointStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AttestationCheckpointMovesToArchived()
    {
        var item = AttestationCheckpoint.NewAttestationCheckpoint("code-122", "label-697");
        try
        {
            item.MoveTo(AttestationCheckpointStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AttestationCheckpointStatus.Archived, "status");
    }

    public static void AttestationCheckpointRejectsMovesFromArchived()
    {
        var item = AttestationCheckpoint.NewAttestationCheckpoint("code-122", "label-697");
        item.Status = AttestationCheckpointStatus.Archived;
        Check.Fails(() => item.MoveTo(AttestationCheckpointStatus.Open), "move");
    }

    public static void RunAll()
    {
        AttestationCheckpointStartsInOpen();
        AttestationCheckpointMovesToArchived();
        AttestationCheckpointRejectsMovesFromArchived();
    }
}
