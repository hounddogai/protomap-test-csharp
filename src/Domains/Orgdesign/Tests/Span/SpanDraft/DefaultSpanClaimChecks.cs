namespace Acme.Workforce.Orgdesign.Tests.Span.SpanDraft;

using Acme.Workforce.Orgdesign.Span.SpanDraft;

/// <summary>Checks of span draft code, run by the project's runner.</summary>
public static class DefaultSpanClaimChecks
{
    public static void DefaultSpanClaimStartsInDraft()
    {
        var item = DefaultSpanClaim.NewDefaultSpanClaim("code-603", "label-489");
        Check.Equal(item.Status, DefaultSpanClaimStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultSpanClaimMovesToActive()
    {
        var item = DefaultSpanClaim.NewDefaultSpanClaim("code-603", "label-489");
        try
        {
            item.MoveTo(DefaultSpanClaimStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultSpanClaimStatus.Active, "status");
    }

    public static void DefaultSpanClaimRejectsMovesFromClosed()
    {
        var item = DefaultSpanClaim.NewDefaultSpanClaim("code-603", "label-489");
        item.Status = DefaultSpanClaimStatus.Closed;
        Check.Fails(() => item.MoveTo(DefaultSpanClaimStatus.Draft), "move");
    }

    public static void RunAll()
    {
        DefaultSpanClaimStartsInDraft();
        DefaultSpanClaimMovesToActive();
        DefaultSpanClaimRejectsMovesFromClosed();
    }
}
