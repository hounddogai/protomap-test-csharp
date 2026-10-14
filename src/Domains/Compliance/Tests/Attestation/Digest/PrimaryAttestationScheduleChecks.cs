namespace Acme.Workforce.Compliance.Tests.Attestation.Digest;

using Acme.Workforce.Compliance.Attestation.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class PrimaryAttestationScheduleChecks
{
    public static void PrimaryAttestationScheduleStartsInProposed()
    {
        var item = PrimaryAttestationSchedule.NewPrimaryAttestationSchedule("code-304", "name-38");
        Check.Equal(item.Status, PrimaryAttestationScheduleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryAttestationScheduleMovesToRejected()
    {
        var item = PrimaryAttestationSchedule.NewPrimaryAttestationSchedule("code-304", "name-38");
        try
        {
            item.MoveTo(PrimaryAttestationScheduleStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryAttestationScheduleStatus.Rejected, "status");
    }

    public static void PrimaryAttestationScheduleRejectsMovesFromWithdrawn()
    {
        var item = PrimaryAttestationSchedule.NewPrimaryAttestationSchedule("code-304", "name-38");
        item.Status = PrimaryAttestationScheduleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PrimaryAttestationScheduleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PrimaryAttestationScheduleStartsInProposed();
        PrimaryAttestationScheduleMovesToRejected();
        PrimaryAttestationScheduleRejectsMovesFromWithdrawn();
    }
}
