namespace Acme.Workforce.Orgdesign.Tests.Span.Slot;

using Acme.Workforce.Orgdesign.Span.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class SpanSlotChecks
{
    public static void SpanSlotStartsInPlanned()
    {
        var item = SpanSlot.NewSpanSlot("code-431", "label-149");
        Check.Equal(item.Status, SpanSlotStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SpanSlotMovesToInProgress()
    {
        var item = SpanSlot.NewSpanSlot("code-431", "label-149");
        try
        {
            item.MoveTo(SpanSlotStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SpanSlotStatus.InProgress, "status");
    }

    public static void SpanSlotRejectsMovesFromCancelled()
    {
        var item = SpanSlot.NewSpanSlot("code-431", "label-149");
        item.Status = SpanSlotStatus.Cancelled;
        Check.Fails(() => item.MoveTo(SpanSlotStatus.Planned), "move");
    }

    public static void RunAll()
    {
        SpanSlotStartsInPlanned();
        SpanSlotMovesToInProgress();
        SpanSlotRejectsMovesFromCancelled();
    }
}
