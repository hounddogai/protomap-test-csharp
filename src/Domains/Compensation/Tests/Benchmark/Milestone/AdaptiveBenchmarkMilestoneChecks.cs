namespace Acme.Workforce.Compensation.Tests.Benchmark.Milestone;

using Acme.Workforce.Compensation.Benchmark.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class AdaptiveBenchmarkMilestoneChecks
{
    public static void AdaptiveBenchmarkMilestoneStartsInPlanned()
    {
        var item = AdaptiveBenchmarkMilestone.NewAdaptiveBenchmarkMilestone("code-682", "name-794");
        Check.Equal(item.Status, AdaptiveBenchmarkMilestoneStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveBenchmarkMilestoneMovesToInProgress()
    {
        var item = AdaptiveBenchmarkMilestone.NewAdaptiveBenchmarkMilestone("code-682", "name-794");
        try
        {
            item.MoveTo(AdaptiveBenchmarkMilestoneStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveBenchmarkMilestoneStatus.InProgress, "status");
    }

    public static void AdaptiveBenchmarkMilestoneRejectsMovesFromCancelled()
    {
        var item = AdaptiveBenchmarkMilestone.NewAdaptiveBenchmarkMilestone("code-682", "name-794");
        item.Status = AdaptiveBenchmarkMilestoneStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AdaptiveBenchmarkMilestoneStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AdaptiveBenchmarkMilestoneStartsInPlanned();
        AdaptiveBenchmarkMilestoneMovesToInProgress();
        AdaptiveBenchmarkMilestoneRejectsMovesFromCancelled();
    }
}
