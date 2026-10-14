namespace Acme.Workforce.Compensation.Tests.Benchmark.Review;

using Acme.Workforce.Compensation.Benchmark.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class LenientBenchmarkRollupChecks
{
    public static void LenientBenchmarkRollupStartsInRequested()
    {
        var item = LenientBenchmarkRollup.NewLenientBenchmarkRollup("code-452", "title-11");
        Check.Equal(item.Status, LenientBenchmarkRollupStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientBenchmarkRollupMovesToGranted()
    {
        var item = LenientBenchmarkRollup.NewLenientBenchmarkRollup("code-452", "title-11");
        try
        {
            item.MoveTo(LenientBenchmarkRollupStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientBenchmarkRollupStatus.Granted, "status");
    }

    public static void LenientBenchmarkRollupRejectsMovesFromExpired()
    {
        var item = LenientBenchmarkRollup.NewLenientBenchmarkRollup("code-452", "title-11");
        item.Status = LenientBenchmarkRollupStatus.Expired;
        Check.Fails(() => item.MoveTo(LenientBenchmarkRollupStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LenientBenchmarkRollupStartsInRequested();
        LenientBenchmarkRollupMovesToGranted();
        LenientBenchmarkRollupRejectsMovesFromExpired();
    }
}
