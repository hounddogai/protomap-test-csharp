namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkAudit;

using Acme.Workforce.Compensation.Benchmark.BenchmarkAudit;

/// <summary>Checks of benchmark audit code, run by the project's runner.</summary>
public static class BenchmarkScheduleChecks
{
    public static void BenchmarkScheduleStartsInNew()
    {
        var item = BenchmarkSchedule.NewBenchmarkSchedule("code-543", "label-757");
        Check.Equal(item.Status, BenchmarkScheduleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BenchmarkScheduleMovesToFailed()
    {
        var item = BenchmarkSchedule.NewBenchmarkSchedule("code-543", "label-757");
        try
        {
            item.MoveTo(BenchmarkScheduleStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BenchmarkScheduleStatus.Failed, "status");
    }

    public static void BenchmarkScheduleRejectsMovesFromFailed()
    {
        var item = BenchmarkSchedule.NewBenchmarkSchedule("code-543", "label-757");
        item.Status = BenchmarkScheduleStatus.Failed;
        Check.Fails(() => item.MoveTo(BenchmarkScheduleStatus.New), "move");
    }

    public static void RunAll()
    {
        BenchmarkScheduleStartsInNew();
        BenchmarkScheduleMovesToFailed();
        BenchmarkScheduleRejectsMovesFromFailed();
    }
}
