namespace Acme.Workforce.Orgdesign.Tests.Span.Claim;

using Acme.Workforce.Orgdesign.Span.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class PendingSpanClaimChecks
{
    public static void PendingSpanClaimStartsInRequested()
    {
        var item = PendingSpanClaim.NewPendingSpanClaim("code-387", "label-432");
        Check.Equal(item.Status, PendingSpanClaimStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingSpanClaimMovesToGranted()
    {
        var item = PendingSpanClaim.NewPendingSpanClaim("code-387", "label-432");
        try
        {
            item.MoveTo(PendingSpanClaimStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingSpanClaimStatus.Granted, "status");
    }

    public static void PendingSpanClaimRejectsMovesFromExpired()
    {
        var item = PendingSpanClaim.NewPendingSpanClaim("code-387", "label-432");
        item.Status = PendingSpanClaimStatus.Expired;
        Check.Fails(() => item.MoveTo(PendingSpanClaimStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PendingSpanClaimStartsInRequested();
        PendingSpanClaimMovesToGranted();
        PendingSpanClaimRejectsMovesFromExpired();
    }
}
