namespace Acme.Workforce.Absence.Tests.Policy.Profile;

using Acme.Workforce.Absence.Policy.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class ScheduledPolicyItemChecks
{
    public static void ScheduledPolicyItemStartsInIdle()
    {
        var item = ScheduledPolicyItem.NewScheduledPolicyItem("code-967", "title-667");
        Check.Equal(item.Status, ScheduledPolicyItemStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledPolicyItemMovesToStopped()
    {
        var item = ScheduledPolicyItem.NewScheduledPolicyItem("code-967", "title-667");
        try
        {
            item.MoveTo(ScheduledPolicyItemStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledPolicyItemStatus.Stopped, "status");
    }

    public static void ScheduledPolicyItemRejectsMovesFromStopped()
    {
        var item = ScheduledPolicyItem.NewScheduledPolicyItem("code-967", "title-667");
        item.Status = ScheduledPolicyItemStatus.Stopped;
        Check.Fails(() => item.MoveTo(ScheduledPolicyItemStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ScheduledPolicyItemStartsInIdle();
        ScheduledPolicyItemMovesToStopped();
        ScheduledPolicyItemRejectsMovesFromStopped();
    }
}
