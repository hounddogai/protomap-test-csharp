namespace Acme.Workforce.Orgdesign.Tests.Span.Claim;

using Acme.Workforce.Orgdesign.Span.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class RollingSpanExceptionChecks
{
    public static void RollingSpanExceptionStartsInPlanned()
    {
        var item = RollingSpanException.NewRollingSpanException("code-811", "title-160");
        Check.Equal(item.Status, RollingSpanExceptionStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingSpanExceptionMovesToCompleted()
    {
        var item = RollingSpanException.NewRollingSpanException("code-811", "title-160");
        try
        {
            item.MoveTo(RollingSpanExceptionStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingSpanExceptionStatus.Completed, "status");
    }

    public static void RollingSpanExceptionRejectsMovesFromCancelled()
    {
        var item = RollingSpanException.NewRollingSpanException("code-811", "title-160");
        item.Status = RollingSpanExceptionStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RollingSpanExceptionStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RollingSpanExceptionStartsInPlanned();
        RollingSpanExceptionMovesToCompleted();
        RollingSpanExceptionRejectsMovesFromCancelled();
    }
}
