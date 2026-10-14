namespace Acme.Workforce.Compliance.Tests.Exception.SeasonalExceptionGroup;

using Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

/// <summary>Checks of seasonal exception group code, run by the project's runner.</summary>
public static class SeasonalExceptionGroupChecks
{
    public static void SeasonalExceptionGroupStartsInDraft()
    {
        var item = SeasonalExceptionGroup.NewSeasonalExceptionGroup("code-16", "label-475");
        Check.Equal(item.Status, SeasonalExceptionGroupStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalExceptionGroupMovesToPending()
    {
        var item = SeasonalExceptionGroup.NewSeasonalExceptionGroup("code-16", "label-475");
        try
        {
            item.MoveTo(SeasonalExceptionGroupStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalExceptionGroupStatus.Pending, "status");
    }

    public static void SeasonalExceptionGroupRejectsMovesFromClosed()
    {
        var item = SeasonalExceptionGroup.NewSeasonalExceptionGroup("code-16", "label-475");
        item.Status = SeasonalExceptionGroupStatus.Closed;
        Check.Fails(() => item.MoveTo(SeasonalExceptionGroupStatus.Draft), "move");
    }

    public static void RunAll()
    {
        SeasonalExceptionGroupStartsInDraft();
        SeasonalExceptionGroupMovesToPending();
        SeasonalExceptionGroupRejectsMovesFromClosed();
    }
}
