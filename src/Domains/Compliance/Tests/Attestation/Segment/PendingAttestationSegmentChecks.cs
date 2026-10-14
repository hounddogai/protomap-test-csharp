namespace Acme.Workforce.Compliance.Tests.Attestation.Segment;

using Acme.Workforce.Compliance.Attestation.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class PendingAttestationSegmentChecks
{
    public static void PendingAttestationSegmentStartsInNew()
    {
        var item = PendingAttestationSegment.NewPendingAttestationSegment("code-294", "title-302");
        Check.Equal(item.Status, PendingAttestationSegmentStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingAttestationSegmentMovesToQueued()
    {
        var item = PendingAttestationSegment.NewPendingAttestationSegment("code-294", "title-302");
        try
        {
            item.MoveTo(PendingAttestationSegmentStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingAttestationSegmentStatus.Queued, "status");
    }

    public static void PendingAttestationSegmentRejectsMovesFromFailed()
    {
        var item = PendingAttestationSegment.NewPendingAttestationSegment("code-294", "title-302");
        item.Status = PendingAttestationSegmentStatus.Failed;
        Check.Fails(() => item.MoveTo(PendingAttestationSegmentStatus.New), "move");
    }

    public static void RunAll()
    {
        PendingAttestationSegmentStartsInNew();
        PendingAttestationSegmentMovesToQueued();
        PendingAttestationSegmentRejectsMovesFromFailed();
    }
}
