namespace Acme.Workforce.Assets.Tests.Refresh.Request;

using Acme.Workforce.Assets.Refresh.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class RollingRefreshPreferenceChecks
{
    public static void RollingRefreshPreferenceStartsInOpen()
    {
        var item = RollingRefreshPreference.NewRollingRefreshPreference("code-129", "label-427");
        Check.Equal(item.Status, RollingRefreshPreferenceStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingRefreshPreferenceMovesToReopened()
    {
        var item = RollingRefreshPreference.NewRollingRefreshPreference("code-129", "label-427");
        try
        {
            item.MoveTo(RollingRefreshPreferenceStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingRefreshPreferenceStatus.Reopened, "status");
    }

    public static void RollingRefreshPreferenceRejectsMovesFromArchived()
    {
        var item = RollingRefreshPreference.NewRollingRefreshPreference("code-129", "label-427");
        item.Status = RollingRefreshPreferenceStatus.Archived;
        Check.Fails(() => item.MoveTo(RollingRefreshPreferenceStatus.Open), "move");
    }

    public static void RunAll()
    {
        RollingRefreshPreferenceStartsInOpen();
        RollingRefreshPreferenceMovesToReopened();
        RollingRefreshPreferenceRejectsMovesFromArchived();
    }
}
