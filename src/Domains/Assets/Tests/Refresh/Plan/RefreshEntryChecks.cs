namespace Acme.Workforce.Assets.Tests.Refresh.Plan;

using Acme.Workforce.Assets.Refresh.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class RefreshEntryChecks
{
    public static void RefreshEntryStartsInNew()
    {
        var item = RefreshEntry.NewRefreshEntry("code-827", "label-149");
        Check.Equal(item.Status, RefreshEntryStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RefreshEntryMovesToFailed()
    {
        var item = RefreshEntry.NewRefreshEntry("code-827", "label-149");
        try
        {
            item.MoveTo(RefreshEntryStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RefreshEntryStatus.Failed, "status");
    }

    public static void RefreshEntryRejectsMovesFromFailed()
    {
        var item = RefreshEntry.NewRefreshEntry("code-827", "label-149");
        item.Status = RefreshEntryStatus.Failed;
        Check.Fails(() => item.MoveTo(RefreshEntryStatus.New), "move");
    }

    public static void RunAll()
    {
        RefreshEntryStartsInNew();
        RefreshEntryMovesToFailed();
        RefreshEntryRejectsMovesFromFailed();
    }
}
