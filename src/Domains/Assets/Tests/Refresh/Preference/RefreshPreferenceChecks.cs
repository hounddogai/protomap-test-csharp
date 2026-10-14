namespace Acme.Workforce.Assets.Tests.Refresh.Preference;

using Acme.Workforce.Assets.Refresh.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RefreshPreferenceChecks
{
    public static void RefreshPreferenceStartsInPlanned()
    {
        var item = RefreshPreference.NewRefreshPreference("code-364", "label-908");
        Check.Equal(item.Status, RefreshPreferenceStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RefreshPreferenceMovesToScheduled()
    {
        var item = RefreshPreference.NewRefreshPreference("code-364", "label-908");
        try
        {
            item.MoveTo(RefreshPreferenceStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RefreshPreferenceStatus.Scheduled, "status");
    }

    public static void RefreshPreferenceRejectsMovesFromCancelled()
    {
        var item = RefreshPreference.NewRefreshPreference("code-364", "label-908");
        item.Status = RefreshPreferenceStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RefreshPreferenceStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RefreshPreferenceStartsInPlanned();
        RefreshPreferenceMovesToScheduled();
        RefreshPreferenceRejectsMovesFromCancelled();
    }
}
