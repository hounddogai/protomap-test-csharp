namespace Acme.Workforce.Orgdesign.Tests.Span.Rollup;

using Acme.Workforce.Orgdesign.Span.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SpanBaselineChecks
{
    public static void SpanBaselineStartsInCreated()
    {
        var item = SpanBaseline.NewSpanBaseline("code-461", "name-42");
        Check.Equal(item.Status, SpanBaselineStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SpanBaselineMovesToValidated()
    {
        var item = SpanBaseline.NewSpanBaseline("code-461", "name-42");
        try
        {
            item.MoveTo(SpanBaselineStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SpanBaselineStatus.Validated, "status");
    }

    public static void SpanBaselineRejectsMovesFromRetired()
    {
        var item = SpanBaseline.NewSpanBaseline("code-461", "name-42");
        item.Status = SpanBaselineStatus.Retired;
        Check.Fails(() => item.MoveTo(SpanBaselineStatus.Created), "move");
    }

    public static void RunAll()
    {
        SpanBaselineStartsInCreated();
        SpanBaselineMovesToValidated();
        SpanBaselineRejectsMovesFromRetired();
    }
}
