namespace Acme.Workforce.Compliance.Tests.Certification.Threshold;

using Acme.Workforce.Compliance.Certification.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class TieredCertificationCheckpointChecks
{
    public static void TieredCertificationCheckpointStartsInProposed()
    {
        var item = TieredCertificationCheckpoint.NewTieredCertificationCheckpoint("code-841", "name-481");
        Check.Equal(item.Status, TieredCertificationCheckpointStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredCertificationCheckpointMovesToApproved()
    {
        var item = TieredCertificationCheckpoint.NewTieredCertificationCheckpoint("code-841", "name-481");
        try
        {
            item.MoveTo(TieredCertificationCheckpointStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredCertificationCheckpointStatus.Approved, "status");
    }

    public static void TieredCertificationCheckpointRejectsMovesFromWithdrawn()
    {
        var item = TieredCertificationCheckpoint.NewTieredCertificationCheckpoint("code-841", "name-481");
        item.Status = TieredCertificationCheckpointStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(TieredCertificationCheckpointStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        TieredCertificationCheckpointStartsInProposed();
        TieredCertificationCheckpointMovesToApproved();
        TieredCertificationCheckpointRejectsMovesFromWithdrawn();
    }
}
