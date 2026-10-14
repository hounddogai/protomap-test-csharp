namespace Acme.Workforce.Compensation.Tests.Benchmark.Metric;

using Acme.Workforce.Compensation.Benchmark.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class RegionalBenchmarkEstimateChecks
{
    public static void RegionalBenchmarkEstimateStartsInRequested()
    {
        var item = RegionalBenchmarkEstimate.NewRegionalBenchmarkEstimate("code-678", "name-841");
        Check.Equal(item.Status, RegionalBenchmarkEstimateStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalBenchmarkEstimateMovesToGranted()
    {
        var item = RegionalBenchmarkEstimate.NewRegionalBenchmarkEstimate("code-678", "name-841");
        try
        {
            item.MoveTo(RegionalBenchmarkEstimateStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalBenchmarkEstimateStatus.Granted, "status");
    }

    public static void RegionalBenchmarkEstimateRejectsMovesFromExpired()
    {
        var item = RegionalBenchmarkEstimate.NewRegionalBenchmarkEstimate("code-678", "name-841");
        item.Status = RegionalBenchmarkEstimateStatus.Expired;
        Check.Fails(() => item.MoveTo(RegionalBenchmarkEstimateStatus.Requested), "move");
    }

    public static void RunAll()
    {
        RegionalBenchmarkEstimateStartsInRequested();
        RegionalBenchmarkEstimateMovesToGranted();
        RegionalBenchmarkEstimateRejectsMovesFromExpired();
    }
}
