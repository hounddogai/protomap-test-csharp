namespace Acme.Workforce.Contractors.Tests.Rate.RateBatch;

using Acme.Workforce.Contractors.Rate.RateBatch;

/// <summary>Checks of rate batch code, run by the project's runner.</summary>
public static class RateMilestoneChecks
{
    public static void RateMilestoneStartsInPlanned()
    {
        var item = RateMilestone.NewRateMilestone("code-412", "name-910");
        Check.Equal(item.Status, RateMilestoneStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RateMilestoneMovesToInProgress()
    {
        var item = RateMilestone.NewRateMilestone("code-412", "name-910");
        try
        {
            item.MoveTo(RateMilestoneStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RateMilestoneStatus.InProgress, "status");
    }

    public static void RateMilestoneRejectsMovesFromCancelled()
    {
        var item = RateMilestone.NewRateMilestone("code-412", "name-910");
        item.Status = RateMilestoneStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RateMilestoneStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RateMilestoneStartsInPlanned();
        RateMilestoneMovesToInProgress();
        RateMilestoneRejectsMovesFromCancelled();
    }
}
