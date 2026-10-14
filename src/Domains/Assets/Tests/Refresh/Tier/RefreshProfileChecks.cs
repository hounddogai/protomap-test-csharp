namespace Acme.Workforce.Assets.Tests.Refresh.Tier;

using Acme.Workforce.Assets.Refresh.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class RefreshProfileChecks
{
    public static void RefreshProfileStartsInDraft()
    {
        var item = RefreshProfile.NewRefreshProfile("code-817", "title-285");
        Check.Equal(item.Status, RefreshProfileStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RefreshProfileMovesToActive()
    {
        var item = RefreshProfile.NewRefreshProfile("code-817", "title-285");
        try
        {
            item.MoveTo(RefreshProfileStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RefreshProfileStatus.Active, "status");
    }

    public static void RefreshProfileRejectsMovesFromClosed()
    {
        var item = RefreshProfile.NewRefreshProfile("code-817", "title-285");
        item.Status = RefreshProfileStatus.Closed;
        Check.Fails(() => item.MoveTo(RefreshProfileStatus.Draft), "move");
    }

    public static void RunAll()
    {
        RefreshProfileStartsInDraft();
        RefreshProfileMovesToActive();
        RefreshProfileRejectsMovesFromClosed();
    }
}
