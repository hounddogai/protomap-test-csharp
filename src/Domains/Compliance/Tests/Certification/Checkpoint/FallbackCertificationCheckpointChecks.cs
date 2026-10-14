namespace Acme.Workforce.Compliance.Tests.Certification.Checkpoint;

using Acme.Workforce.Compliance.Certification.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class FallbackCertificationCheckpointChecks
{
    public static void FallbackCertificationCheckpointStartsInProposed()
    {
        var item = FallbackCertificationCheckpoint.NewFallbackCertificationCheckpoint("code-613", "title-620");
        Check.Equal(item.Status, FallbackCertificationCheckpointStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackCertificationCheckpointMovesToApproved()
    {
        var item = FallbackCertificationCheckpoint.NewFallbackCertificationCheckpoint("code-613", "title-620");
        try
        {
            item.MoveTo(FallbackCertificationCheckpointStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackCertificationCheckpointStatus.Approved, "status");
    }

    public static void FallbackCertificationCheckpointRejectsMovesFromWithdrawn()
    {
        var item = FallbackCertificationCheckpoint.NewFallbackCertificationCheckpoint("code-613", "title-620");
        item.Status = FallbackCertificationCheckpointStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(FallbackCertificationCheckpointStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        FallbackCertificationCheckpointStartsInProposed();
        FallbackCertificationCheckpointMovesToApproved();
        FallbackCertificationCheckpointRejectsMovesFromWithdrawn();
    }
}
