namespace Acme.Workforce.Assets.Tests.Refresh.OverrideItem;

using Acme.Workforce.Assets.Refresh.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RefreshOverrideChecks
{
    public static void RefreshOverrideStartsInNew()
    {
        var item = RefreshOverride.NewRefreshOverride("code-616", "label-528");
        Check.Equal(item.Status, RefreshOverrideStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RefreshOverrideMovesToQueued()
    {
        var item = RefreshOverride.NewRefreshOverride("code-616", "label-528");
        try
        {
            item.MoveTo(RefreshOverrideStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RefreshOverrideStatus.Queued, "status");
    }

    public static void RefreshOverrideRejectsMovesFromFailed()
    {
        var item = RefreshOverride.NewRefreshOverride("code-616", "label-528");
        item.Status = RefreshOverrideStatus.Failed;
        Check.Fails(() => item.MoveTo(RefreshOverrideStatus.New), "move");
    }

    public static void RunAll()
    {
        RefreshOverrideStartsInNew();
        RefreshOverrideMovesToQueued();
        RefreshOverrideRejectsMovesFromFailed();
    }
}
