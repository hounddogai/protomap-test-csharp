namespace Acme.Workforce.Assets.Tests.Refresh.RefreshReview;

using Acme.Workforce.Assets.Refresh.RefreshReview;

/// <summary>Checks of refresh review code, run by the project's runner.</summary>
public static class LegacyRefreshProfileChecks
{
    public static void LegacyRefreshProfileStartsInPlanned()
    {
        var item = LegacyRefreshProfile.NewLegacyRefreshProfile("code-50", "label-555");
        Check.Equal(item.Status, LegacyRefreshProfileStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyRefreshProfileMovesToInProgress()
    {
        var item = LegacyRefreshProfile.NewLegacyRefreshProfile("code-50", "label-555");
        try
        {
            item.MoveTo(LegacyRefreshProfileStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyRefreshProfileStatus.InProgress, "status");
    }

    public static void LegacyRefreshProfileRejectsMovesFromCancelled()
    {
        var item = LegacyRefreshProfile.NewLegacyRefreshProfile("code-50", "label-555");
        item.Status = LegacyRefreshProfileStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LegacyRefreshProfileStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LegacyRefreshProfileStartsInPlanned();
        LegacyRefreshProfileMovesToInProgress();
        LegacyRefreshProfileRejectsMovesFromCancelled();
    }
}
