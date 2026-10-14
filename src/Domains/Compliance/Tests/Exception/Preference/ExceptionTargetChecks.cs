namespace Acme.Workforce.Compliance.Tests.Exception.Preference;

using Acme.Workforce.Compliance.Exception.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ExceptionTargetChecks
{
    public static void ExceptionTargetStartsInDraft()
    {
        var item = ExceptionTarget.NewExceptionTarget("code-359", "label-952");
        Check.Equal(item.Status, ExceptionTargetStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ExceptionTargetMovesToClosed()
    {
        var item = ExceptionTarget.NewExceptionTarget("code-359", "label-952");
        try
        {
            item.MoveTo(ExceptionTargetStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ExceptionTargetStatus.Closed, "status");
    }

    public static void ExceptionTargetRejectsMovesFromClosed()
    {
        var item = ExceptionTarget.NewExceptionTarget("code-359", "label-952");
        item.Status = ExceptionTargetStatus.Closed;
        Check.Fails(() => item.MoveTo(ExceptionTargetStatus.Draft), "move");
    }

    public static void RunAll()
    {
        ExceptionTargetStartsInDraft();
        ExceptionTargetMovesToClosed();
        ExceptionTargetRejectsMovesFromClosed();
    }
}
