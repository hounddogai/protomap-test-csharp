namespace Acme.Workforce.Orgdesign.Tests.Span.Exception;

using Acme.Workforce.Orgdesign.Span.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class SpanExceptionChecks
{
    public static void SpanExceptionStartsInProposed()
    {
        var item = SpanException.NewSpanException("code-887", "label-725");
        Check.Equal(item.Status, SpanExceptionStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SpanExceptionMovesToRejected()
    {
        var item = SpanException.NewSpanException("code-887", "label-725");
        try
        {
            item.MoveTo(SpanExceptionStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SpanExceptionStatus.Rejected, "status");
    }

    public static void SpanExceptionRejectsMovesFromWithdrawn()
    {
        var item = SpanException.NewSpanException("code-887", "label-725");
        item.Status = SpanExceptionStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(SpanExceptionStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        SpanExceptionStartsInProposed();
        SpanExceptionMovesToRejected();
        SpanExceptionRejectsMovesFromWithdrawn();
    }
}
