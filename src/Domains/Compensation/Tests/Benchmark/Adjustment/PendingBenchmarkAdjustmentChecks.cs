namespace Acme.Workforce.Compensation.Tests.Benchmark.Adjustment;

using Acme.Workforce.Compensation.Benchmark.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class PendingBenchmarkAdjustmentChecks
{
    public static void PendingBenchmarkAdjustmentStartsInRequested()
    {
        var item = PendingBenchmarkAdjustment.NewPendingBenchmarkAdjustment("code-760", "title-586");
        Check.Equal(item.Status, PendingBenchmarkAdjustmentStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingBenchmarkAdjustmentMovesToExpired()
    {
        var item = PendingBenchmarkAdjustment.NewPendingBenchmarkAdjustment("code-760", "title-586");
        try
        {
            item.MoveTo(PendingBenchmarkAdjustmentStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingBenchmarkAdjustmentStatus.Expired, "status");
    }

    public static void PendingBenchmarkAdjustmentRejectsMovesFromExpired()
    {
        var item = PendingBenchmarkAdjustment.NewPendingBenchmarkAdjustment("code-760", "title-586");
        item.Status = PendingBenchmarkAdjustmentStatus.Expired;
        Check.Fails(() => item.MoveTo(PendingBenchmarkAdjustmentStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PendingBenchmarkAdjustmentStartsInRequested();
        PendingBenchmarkAdjustmentMovesToExpired();
        PendingBenchmarkAdjustmentRejectsMovesFromExpired();
    }
}
