namespace Acme.Workforce.Compensation.Tests.Benchmark.Rule;

using Acme.Workforce.Compensation.Benchmark.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class BenchmarkAdjustmentChecks
{
    public static void BenchmarkAdjustmentStartsInCreated()
    {
        var item = BenchmarkAdjustment.NewBenchmarkAdjustment("code-319", "label-336");
        Check.Equal(item.Status, BenchmarkAdjustmentStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BenchmarkAdjustmentMovesToPublished()
    {
        var item = BenchmarkAdjustment.NewBenchmarkAdjustment("code-319", "label-336");
        try
        {
            item.MoveTo(BenchmarkAdjustmentStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BenchmarkAdjustmentStatus.Published, "status");
    }

    public static void BenchmarkAdjustmentRejectsMovesFromRetired()
    {
        var item = BenchmarkAdjustment.NewBenchmarkAdjustment("code-319", "label-336");
        item.Status = BenchmarkAdjustmentStatus.Retired;
        Check.Fails(() => item.MoveTo(BenchmarkAdjustmentStatus.Created), "move");
    }

    public static void RunAll()
    {
        BenchmarkAdjustmentStartsInCreated();
        BenchmarkAdjustmentMovesToPublished();
        BenchmarkAdjustmentRejectsMovesFromRetired();
    }
}
