namespace Acme.Workforce.Compensation.Tests.Benchmark.Review;

using Acme.Workforce.Compensation.Benchmark.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class ScheduledBenchmarkScheduleChecks
{
    public static void ScheduledBenchmarkScheduleStartsInNew()
    {
        var item = ScheduledBenchmarkSchedule.NewScheduledBenchmarkSchedule("code-16", "label-237");
        Check.Equal(item.Status, ScheduledBenchmarkScheduleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledBenchmarkScheduleMovesToQueued()
    {
        var item = ScheduledBenchmarkSchedule.NewScheduledBenchmarkSchedule("code-16", "label-237");
        try
        {
            item.MoveTo(ScheduledBenchmarkScheduleStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledBenchmarkScheduleStatus.Queued, "status");
    }

    public static void ScheduledBenchmarkScheduleRejectsMovesFromFailed()
    {
        var item = ScheduledBenchmarkSchedule.NewScheduledBenchmarkSchedule("code-16", "label-237");
        item.Status = ScheduledBenchmarkScheduleStatus.Failed;
        Check.Fails(() => item.MoveTo(ScheduledBenchmarkScheduleStatus.New), "move");
    }

    public static void RunAll()
    {
        ScheduledBenchmarkScheduleStartsInNew();
        ScheduledBenchmarkScheduleMovesToQueued();
        ScheduledBenchmarkScheduleRejectsMovesFromFailed();
    }
}
