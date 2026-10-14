namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementAssignment;

using Acme.Workforce.Contractors.Engagement.EngagementAssignment;

/// <summary>Checks of engagement assignment code, run by the project's runner.</summary>
public static class DefaultEngagementPreferenceChecks
{
    public static void DefaultEngagementPreferenceStartsInPlanned()
    {
        var item = DefaultEngagementPreference.NewDefaultEngagementPreference("code-882", "name-335");
        Check.Equal(item.Status, DefaultEngagementPreferenceStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultEngagementPreferenceMovesToInProgress()
    {
        var item = DefaultEngagementPreference.NewDefaultEngagementPreference("code-882", "name-335");
        try
        {
            item.MoveTo(DefaultEngagementPreferenceStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultEngagementPreferenceStatus.InProgress, "status");
    }

    public static void DefaultEngagementPreferenceRejectsMovesFromCancelled()
    {
        var item = DefaultEngagementPreference.NewDefaultEngagementPreference("code-882", "name-335");
        item.Status = DefaultEngagementPreferenceStatus.Cancelled;
        Check.Fails(() => item.MoveTo(DefaultEngagementPreferenceStatus.Planned), "move");
    }

    public static void RunAll()
    {
        DefaultEngagementPreferenceStartsInPlanned();
        DefaultEngagementPreferenceMovesToInProgress();
        DefaultEngagementPreferenceRejectsMovesFromCancelled();
    }
}
