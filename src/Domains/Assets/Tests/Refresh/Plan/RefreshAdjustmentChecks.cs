namespace Acme.Workforce.Assets.Tests.Refresh.Plan;

using Acme.Workforce.Assets.Refresh.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class RefreshAdjustmentChecks
{
    public static void RefreshAdjustmentStartsInDraft()
    {
        var item = RefreshAdjustment.NewRefreshAdjustment("code-269", "label-790");
        Check.Equal(item.Status, RefreshAdjustmentStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RefreshAdjustmentMovesToSuspended()
    {
        var item = RefreshAdjustment.NewRefreshAdjustment("code-269", "label-790");
        try
        {
            item.MoveTo(RefreshAdjustmentStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RefreshAdjustmentStatus.Suspended, "status");
    }

    public static void RefreshAdjustmentRejectsMovesFromClosed()
    {
        var item = RefreshAdjustment.NewRefreshAdjustment("code-269", "label-790");
        item.Status = RefreshAdjustmentStatus.Closed;
        Check.Fails(() => item.MoveTo(RefreshAdjustmentStatus.Draft), "move");
    }

    public static void RunAll()
    {
        RefreshAdjustmentStartsInDraft();
        RefreshAdjustmentMovesToSuspended();
        RefreshAdjustmentRejectsMovesFromClosed();
    }
}
