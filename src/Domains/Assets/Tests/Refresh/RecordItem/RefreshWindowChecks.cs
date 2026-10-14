namespace Acme.Workforce.Assets.Tests.Refresh.RecordItem;

using Acme.Workforce.Assets.Refresh.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class RefreshWindowChecks
{
    public static void RefreshWindowStartsInPlanned()
    {
        var item = RefreshWindow.NewRefreshWindow("code-698", "title-985");
        Check.Equal(item.Status, RefreshWindowStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RefreshWindowMovesToScheduled()
    {
        var item = RefreshWindow.NewRefreshWindow("code-698", "title-985");
        try
        {
            item.MoveTo(RefreshWindowStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RefreshWindowStatus.Scheduled, "status");
    }

    public static void RefreshWindowRejectsMovesFromCancelled()
    {
        var item = RefreshWindow.NewRefreshWindow("code-698", "title-985");
        item.Status = RefreshWindowStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RefreshWindowStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RefreshWindowStartsInPlanned();
        RefreshWindowMovesToScheduled();
        RefreshWindowRejectsMovesFromCancelled();
    }
}
