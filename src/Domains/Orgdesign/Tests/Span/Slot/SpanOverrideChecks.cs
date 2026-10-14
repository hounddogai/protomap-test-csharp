namespace Acme.Workforce.Orgdesign.Tests.Span.Slot;

using Acme.Workforce.Orgdesign.Span.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class SpanOverrideChecks
{
    public static void SpanOverrideStartsInNew()
    {
        var item = SpanOverride.NewSpanOverride("code-335", "label-55");
        Check.Equal(item.Status, SpanOverrideStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SpanOverrideMovesToFailed()
    {
        var item = SpanOverride.NewSpanOverride("code-335", "label-55");
        try
        {
            item.MoveTo(SpanOverrideStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SpanOverrideStatus.Failed, "status");
    }

    public static void SpanOverrideRejectsMovesFromFailed()
    {
        var item = SpanOverride.NewSpanOverride("code-335", "label-55");
        item.Status = SpanOverrideStatus.Failed;
        Check.Fails(() => item.MoveTo(SpanOverrideStatus.New), "move");
    }

    public static void RunAll()
    {
        SpanOverrideStartsInNew();
        SpanOverrideMovesToFailed();
        SpanOverrideRejectsMovesFromFailed();
    }
}
