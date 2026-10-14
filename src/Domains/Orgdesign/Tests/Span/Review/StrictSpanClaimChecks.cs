namespace Acme.Workforce.Orgdesign.Tests.Span.Review;

using Acme.Workforce.Orgdesign.Span.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class StrictSpanClaimChecks
{
    public static void StrictSpanClaimStartsInPlanned()
    {
        var item = StrictSpanClaim.NewStrictSpanClaim("code-45", "name-799");
        Check.Equal(item.Status, StrictSpanClaimStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictSpanClaimMovesToScheduled()
    {
        var item = StrictSpanClaim.NewStrictSpanClaim("code-45", "name-799");
        try
        {
            item.MoveTo(StrictSpanClaimStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictSpanClaimStatus.Scheduled, "status");
    }

    public static void StrictSpanClaimRejectsMovesFromCancelled()
    {
        var item = StrictSpanClaim.NewStrictSpanClaim("code-45", "name-799");
        item.Status = StrictSpanClaimStatus.Cancelled;
        Check.Fails(() => item.MoveTo(StrictSpanClaimStatus.Planned), "move");
    }

    public static void RunAll()
    {
        StrictSpanClaimStartsInPlanned();
        StrictSpanClaimMovesToScheduled();
        StrictSpanClaimRejectsMovesFromCancelled();
    }
}
