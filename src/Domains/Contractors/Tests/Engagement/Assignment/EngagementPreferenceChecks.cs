namespace Acme.Workforce.Contractors.Tests.Engagement.Assignment;

using Acme.Workforce.Contractors.Engagement.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class EngagementPreferenceChecks
{
    public static void EngagementPreferenceStartsInProposed()
    {
        var item = EngagementPreference.NewEngagementPreference("code-908", "name-996");
        Check.Equal(item.Status, EngagementPreferenceStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EngagementPreferenceMovesToRejected()
    {
        var item = EngagementPreference.NewEngagementPreference("code-908", "name-996");
        try
        {
            item.MoveTo(EngagementPreferenceStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EngagementPreferenceStatus.Rejected, "status");
    }

    public static void EngagementPreferenceRejectsMovesFromWithdrawn()
    {
        var item = EngagementPreference.NewEngagementPreference("code-908", "name-996");
        item.Status = EngagementPreferenceStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(EngagementPreferenceStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        EngagementPreferenceStartsInProposed();
        EngagementPreferenceMovesToRejected();
        EngagementPreferenceRejectsMovesFromWithdrawn();
    }
}
