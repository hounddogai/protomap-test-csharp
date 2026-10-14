namespace Acme.Workforce.Orgdesign.Tests.Span.Mapping;

using Acme.Workforce.Orgdesign.Span.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class SpanMappingChecks
{
    public static void SpanMappingStartsInPlanned()
    {
        var item = SpanMapping.NewSpanMapping("code-68", "label-44");
        Check.Equal(item.Status, SpanMappingStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SpanMappingMovesToInProgress()
    {
        var item = SpanMapping.NewSpanMapping("code-68", "label-44");
        try
        {
            item.MoveTo(SpanMappingStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SpanMappingStatus.InProgress, "status");
    }

    public static void SpanMappingRejectsMovesFromCancelled()
    {
        var item = SpanMapping.NewSpanMapping("code-68", "label-44");
        item.Status = SpanMappingStatus.Cancelled;
        Check.Fails(() => item.MoveTo(SpanMappingStatus.Planned), "move");
    }

    public static void RunAll()
    {
        SpanMappingStartsInPlanned();
        SpanMappingMovesToInProgress();
        SpanMappingRejectsMovesFromCancelled();
    }
}
