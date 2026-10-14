namespace Acme.Workforce.Orgdesign.Tests.Span.Metric;

using Acme.Workforce.Orgdesign.Span.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class SpanWindowChecks
{
    public static void SpanWindowStartsInDraft()
    {
        var item = SpanWindow.NewSpanWindow("code-258", "title-6");
        Check.Equal(item.Status, SpanWindowStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SpanWindowMovesToActive()
    {
        var item = SpanWindow.NewSpanWindow("code-258", "title-6");
        try
        {
            item.MoveTo(SpanWindowStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SpanWindowStatus.Active, "status");
    }

    public static void SpanWindowRejectsMovesFromClosed()
    {
        var item = SpanWindow.NewSpanWindow("code-258", "title-6");
        item.Status = SpanWindowStatus.Closed;
        Check.Fails(() => item.MoveTo(SpanWindowStatus.Draft), "move");
    }

    public static void RunAll()
    {
        SpanWindowStartsInDraft();
        SpanWindowMovesToActive();
        SpanWindowRejectsMovesFromClosed();
    }
}
