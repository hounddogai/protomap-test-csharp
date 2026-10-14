namespace Acme.Workforce.Compliance.Tests.Exception.Rule;

using Acme.Workforce.Compliance.Exception.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class ExceptionEntryChecks
{
    public static void ExceptionEntryStartsInNew()
    {
        var item = ExceptionEntry.NewExceptionEntry("code-104", "title-410");
        Check.Equal(item.Status, ExceptionEntryStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ExceptionEntryMovesToQueued()
    {
        var item = ExceptionEntry.NewExceptionEntry("code-104", "title-410");
        try
        {
            item.MoveTo(ExceptionEntryStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ExceptionEntryStatus.Queued, "status");
    }

    public static void ExceptionEntryRejectsMovesFromFailed()
    {
        var item = ExceptionEntry.NewExceptionEntry("code-104", "title-410");
        item.Status = ExceptionEntryStatus.Failed;
        Check.Fails(() => item.MoveTo(ExceptionEntryStatus.New), "move");
    }

    public static void RunAll()
    {
        ExceptionEntryStartsInNew();
        ExceptionEntryMovesToQueued();
        ExceptionEntryRejectsMovesFromFailed();
    }
}
