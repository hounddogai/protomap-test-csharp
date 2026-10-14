namespace Acme.Workforce.Engagement.Tests.Feedback.Checkpoint;

using Acme.Workforce.Engagement.Feedback.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class SeasonalFeedbackMilestoneChecks
{
    public static void SeasonalFeedbackMilestoneStartsInIdle()
    {
        var item = SeasonalFeedbackMilestone.NewSeasonalFeedbackMilestone("code-620", "name-474");
        Check.Equal(item.Status, SeasonalFeedbackMilestoneStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalFeedbackMilestoneMovesToWarming()
    {
        var item = SeasonalFeedbackMilestone.NewSeasonalFeedbackMilestone("code-620", "name-474");
        try
        {
            item.MoveTo(SeasonalFeedbackMilestoneStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalFeedbackMilestoneStatus.Warming, "status");
    }

    public static void SeasonalFeedbackMilestoneRejectsMovesFromStopped()
    {
        var item = SeasonalFeedbackMilestone.NewSeasonalFeedbackMilestone("code-620", "name-474");
        item.Status = SeasonalFeedbackMilestoneStatus.Stopped;
        Check.Fails(() => item.MoveTo(SeasonalFeedbackMilestoneStatus.Idle), "move");
    }

    public static void RunAll()
    {
        SeasonalFeedbackMilestoneStartsInIdle();
        SeasonalFeedbackMilestoneMovesToWarming();
        SeasonalFeedbackMilestoneRejectsMovesFromStopped();
    }
}
