namespace Acme.Workforce.Orgdesign.Tests.Span.Entry;

using Acme.Workforce.Orgdesign.Span.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class CappedSpanSlotChecks
{
    public static void CappedSpanSlotStartsInDraft()
    {
        var item = CappedSpanSlot.NewCappedSpanSlot("code-890", "label-457");
        Check.Equal(item.Status, CappedSpanSlotStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedSpanSlotMovesToPending()
    {
        var item = CappedSpanSlot.NewCappedSpanSlot("code-890", "label-457");
        try
        {
            item.MoveTo(CappedSpanSlotStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedSpanSlotStatus.Pending, "status");
    }

    public static void CappedSpanSlotRejectsMovesFromClosed()
    {
        var item = CappedSpanSlot.NewCappedSpanSlot("code-890", "label-457");
        item.Status = CappedSpanSlotStatus.Closed;
        Check.Fails(() => item.MoveTo(CappedSpanSlotStatus.Draft), "move");
    }

    public static void RunAll()
    {
        CappedSpanSlotStartsInDraft();
        CappedSpanSlotMovesToPending();
        CappedSpanSlotRejectsMovesFromClosed();
    }
}
