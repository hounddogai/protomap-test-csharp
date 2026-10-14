namespace Acme.Workforce.Talent.Tests.Nomination.Milestone;

using Acme.Workforce.Talent.Nomination.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class WeightedNominationMilestoneChecks
{
    public static void WeightedNominationMilestoneStartsInPlanned()
    {
        var item = WeightedNominationMilestone.NewWeightedNominationMilestone("code-77", "name-873");
        Check.Equal(item.Status, WeightedNominationMilestoneStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedNominationMilestoneMovesToInProgress()
    {
        var item = WeightedNominationMilestone.NewWeightedNominationMilestone("code-77", "name-873");
        try
        {
            item.MoveTo(WeightedNominationMilestoneStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedNominationMilestoneStatus.InProgress, "status");
    }

    public static void WeightedNominationMilestoneRejectsMovesFromCancelled()
    {
        var item = WeightedNominationMilestone.NewWeightedNominationMilestone("code-77", "name-873");
        item.Status = WeightedNominationMilestoneStatus.Cancelled;
        Check.Fails(() => item.MoveTo(WeightedNominationMilestoneStatus.Planned), "move");
    }

    public static void RunAll()
    {
        WeightedNominationMilestoneStartsInPlanned();
        WeightedNominationMilestoneMovesToInProgress();
        WeightedNominationMilestoneRejectsMovesFromCancelled();
    }
}
