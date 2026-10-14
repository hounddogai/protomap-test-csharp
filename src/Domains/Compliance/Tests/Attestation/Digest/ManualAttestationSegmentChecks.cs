namespace Acme.Workforce.Compliance.Tests.Attestation.Digest;

using Acme.Workforce.Compliance.Attestation.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class ManualAttestationSegmentChecks
{
    public static void ManualAttestationSegmentStartsInProposed()
    {
        var item = ManualAttestationSegment.NewManualAttestationSegment("code-80", "name-97");
        Check.Equal(item.Status, ManualAttestationSegmentStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualAttestationSegmentMovesToRejected()
    {
        var item = ManualAttestationSegment.NewManualAttestationSegment("code-80", "name-97");
        try
        {
            item.MoveTo(ManualAttestationSegmentStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualAttestationSegmentStatus.Rejected, "status");
    }

    public static void ManualAttestationSegmentRejectsMovesFromWithdrawn()
    {
        var item = ManualAttestationSegment.NewManualAttestationSegment("code-80", "name-97");
        item.Status = ManualAttestationSegmentStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ManualAttestationSegmentStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ManualAttestationSegmentStartsInProposed();
        ManualAttestationSegmentMovesToRejected();
        ManualAttestationSegmentRejectsMovesFromWithdrawn();
    }
}
