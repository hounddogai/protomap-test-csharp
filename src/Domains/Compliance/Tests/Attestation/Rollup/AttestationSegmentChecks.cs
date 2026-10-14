namespace Acme.Workforce.Compliance.Tests.Attestation.Rollup;

using Acme.Workforce.Compliance.Attestation.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class AttestationSegmentChecks
{
    public static void AttestationSegmentStartsInRequested()
    {
        var item = AttestationSegment.NewAttestationSegment("code-221", "name-944");
        Check.Equal(item.Status, AttestationSegmentStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AttestationSegmentMovesToGranted()
    {
        var item = AttestationSegment.NewAttestationSegment("code-221", "name-944");
        try
        {
            item.MoveTo(AttestationSegmentStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AttestationSegmentStatus.Granted, "status");
    }

    public static void AttestationSegmentRejectsMovesFromExpired()
    {
        var item = AttestationSegment.NewAttestationSegment("code-221", "name-944");
        item.Status = AttestationSegmentStatus.Expired;
        Check.Fails(() => item.MoveTo(AttestationSegmentStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AttestationSegmentStartsInRequested();
        AttestationSegmentMovesToGranted();
        AttestationSegmentRejectsMovesFromExpired();
    }
}
