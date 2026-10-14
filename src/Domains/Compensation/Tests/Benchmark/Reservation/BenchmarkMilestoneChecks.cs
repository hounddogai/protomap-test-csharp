namespace Acme.Workforce.Compensation.Tests.Benchmark.Reservation;

using Acme.Workforce.Compensation.Benchmark.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class BenchmarkMilestoneChecks
{
    public static void BenchmarkMilestoneStartsInPlanned()
    {
        var item = BenchmarkMilestone.NewBenchmarkMilestone("code-731", "label-984");
        Check.Equal(item.Status, BenchmarkMilestoneStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BenchmarkMilestoneMovesToInProgress()
    {
        var item = BenchmarkMilestone.NewBenchmarkMilestone("code-731", "label-984");
        try
        {
            item.MoveTo(BenchmarkMilestoneStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BenchmarkMilestoneStatus.InProgress, "status");
    }

    public static void BenchmarkMilestoneRejectsMovesFromCancelled()
    {
        var item = BenchmarkMilestone.NewBenchmarkMilestone("code-731", "label-984");
        item.Status = BenchmarkMilestoneStatus.Cancelled;
        Check.Fails(() => item.MoveTo(BenchmarkMilestoneStatus.Planned), "move");
    }

    public static void RunAll()
    {
        BenchmarkMilestoneStartsInPlanned();
        BenchmarkMilestoneMovesToInProgress();
        BenchmarkMilestoneRejectsMovesFromCancelled();
    }
}
