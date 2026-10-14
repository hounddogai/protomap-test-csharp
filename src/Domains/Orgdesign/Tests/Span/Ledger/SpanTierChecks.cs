namespace Acme.Workforce.Orgdesign.Tests.Span.Ledger;

using Acme.Workforce.Orgdesign.Span.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class SpanTierChecks
{
    public static void SpanTierStartsInDraft()
    {
        var item = SpanTier.NewSpanTier("code-792", "name-347");
        Check.Equal(item.Status, SpanTierStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SpanTierMovesToPending()
    {
        var item = SpanTier.NewSpanTier("code-792", "name-347");
        try
        {
            item.MoveTo(SpanTierStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SpanTierStatus.Pending, "status");
    }

    public static void SpanTierRejectsMovesFromClosed()
    {
        var item = SpanTier.NewSpanTier("code-792", "name-347");
        item.Status = SpanTierStatus.Closed;
        Check.Fails(() => item.MoveTo(SpanTierStatus.Draft), "move");
    }

    public static void RunAll()
    {
        SpanTierStartsInDraft();
        SpanTierMovesToPending();
        SpanTierRejectsMovesFromClosed();
    }
}
