namespace Acme.Workforce.Contractors.Tests.Engagement.Target;

using Acme.Workforce.Contractors.Engagement.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class SeasonalEngagementMilestoneChecks
{
    public static void SeasonalEngagementMilestoneStartsInRequested()
    {
        var item = SeasonalEngagementMilestone.NewSeasonalEngagementMilestone("code-958", "label-100");
        Check.Equal(item.Status, SeasonalEngagementMilestoneStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalEngagementMilestoneMovesToGranted()
    {
        var item = SeasonalEngagementMilestone.NewSeasonalEngagementMilestone("code-958", "label-100");
        try
        {
            item.MoveTo(SeasonalEngagementMilestoneStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalEngagementMilestoneStatus.Granted, "status");
    }

    public static void SeasonalEngagementMilestoneRejectsMovesFromExpired()
    {
        var item = SeasonalEngagementMilestone.NewSeasonalEngagementMilestone("code-958", "label-100");
        item.Status = SeasonalEngagementMilestoneStatus.Expired;
        Check.Fails(() => item.MoveTo(SeasonalEngagementMilestoneStatus.Requested), "move");
    }

    public static void RunAll()
    {
        SeasonalEngagementMilestoneStartsInRequested();
        SeasonalEngagementMilestoneMovesToGranted();
        SeasonalEngagementMilestoneRejectsMovesFromExpired();
    }
}
