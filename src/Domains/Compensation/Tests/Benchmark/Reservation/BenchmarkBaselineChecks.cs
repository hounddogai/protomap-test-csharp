namespace Acme.Workforce.Compensation.Tests.Benchmark.Reservation;

using Acme.Workforce.Compensation.Benchmark.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class BenchmarkBaselineChecks
{
    public static void BenchmarkBaselineStartsInProposed()
    {
        var item = BenchmarkBaseline.NewBenchmarkBaseline("code-497", "title-646");
        Check.Equal(item.Status, BenchmarkBaselineStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BenchmarkBaselineMovesToApproved()
    {
        var item = BenchmarkBaseline.NewBenchmarkBaseline("code-497", "title-646");
        try
        {
            item.MoveTo(BenchmarkBaselineStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BenchmarkBaselineStatus.Approved, "status");
    }

    public static void BenchmarkBaselineRejectsMovesFromWithdrawn()
    {
        var item = BenchmarkBaseline.NewBenchmarkBaseline("code-497", "title-646");
        item.Status = BenchmarkBaselineStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(BenchmarkBaselineStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        BenchmarkBaselineStartsInProposed();
        BenchmarkBaselineMovesToApproved();
        BenchmarkBaselineRejectsMovesFromWithdrawn();
    }
}
