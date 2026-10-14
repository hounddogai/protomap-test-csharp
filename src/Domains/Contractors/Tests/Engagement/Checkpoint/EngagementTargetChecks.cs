namespace Acme.Workforce.Contractors.Tests.Engagement.Checkpoint;

using Acme.Workforce.Contractors.Engagement.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class EngagementTargetChecks
{
    public static void EngagementTargetStartsInRequested()
    {
        var item = EngagementTarget.NewEngagementTarget("code-320", "name-293");
        Check.Equal(item.Status, EngagementTargetStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EngagementTargetMovesToGranted()
    {
        var item = EngagementTarget.NewEngagementTarget("code-320", "name-293");
        try
        {
            item.MoveTo(EngagementTargetStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EngagementTargetStatus.Granted, "status");
    }

    public static void EngagementTargetRejectsMovesFromExpired()
    {
        var item = EngagementTarget.NewEngagementTarget("code-320", "name-293");
        item.Status = EngagementTargetStatus.Expired;
        Check.Fails(() => item.MoveTo(EngagementTargetStatus.Requested), "move");
    }

    public static void RunAll()
    {
        EngagementTargetStartsInRequested();
        EngagementTargetMovesToGranted();
        EngagementTargetRejectsMovesFromExpired();
    }
}
