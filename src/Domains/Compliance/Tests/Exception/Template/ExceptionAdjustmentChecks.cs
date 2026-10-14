namespace Acme.Workforce.Compliance.Tests.Exception.Template;

using Acme.Workforce.Compliance.Exception.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ExceptionAdjustmentChecks
{
    public static void ExceptionAdjustmentStartsInDraft()
    {
        var item = ExceptionAdjustment.NewExceptionAdjustment("code-459", "title-922");
        Check.Equal(item.Status, ExceptionAdjustmentStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ExceptionAdjustmentMovesToSuspended()
    {
        var item = ExceptionAdjustment.NewExceptionAdjustment("code-459", "title-922");
        try
        {
            item.MoveTo(ExceptionAdjustmentStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ExceptionAdjustmentStatus.Suspended, "status");
    }

    public static void ExceptionAdjustmentRejectsMovesFromClosed()
    {
        var item = ExceptionAdjustment.NewExceptionAdjustment("code-459", "title-922");
        item.Status = ExceptionAdjustmentStatus.Closed;
        Check.Fails(() => item.MoveTo(ExceptionAdjustmentStatus.Draft), "move");
    }

    public static void RunAll()
    {
        ExceptionAdjustmentStartsInDraft();
        ExceptionAdjustmentMovesToSuspended();
        ExceptionAdjustmentRejectsMovesFromClosed();
    }
}
