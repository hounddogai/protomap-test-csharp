namespace Acme.Workforce.Compensation.Tests.Benchmark.AdaptiveBenchmarkAdjustment;

using Acme.Workforce.Compensation.Benchmark.AdaptiveBenchmarkAdjustment;

/// <summary>Checks of adaptive benchmark adjustment code, run by the project's runner.</summary>
public static class SeasonalBenchmarkGroupChecks
{
    public static void SeasonalBenchmarkGroupStartsInCreated()
    {
        var item = SeasonalBenchmarkGroup.NewSeasonalBenchmarkGroup("code-81", "name-540");
        Check.Equal(item.Status, SeasonalBenchmarkGroupStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalBenchmarkGroupMovesToValidated()
    {
        var item = SeasonalBenchmarkGroup.NewSeasonalBenchmarkGroup("code-81", "name-540");
        try
        {
            item.MoveTo(SeasonalBenchmarkGroupStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalBenchmarkGroupStatus.Validated, "status");
    }

    public static void SeasonalBenchmarkGroupRejectsMovesFromRetired()
    {
        var item = SeasonalBenchmarkGroup.NewSeasonalBenchmarkGroup("code-81", "name-540");
        item.Status = SeasonalBenchmarkGroupStatus.Retired;
        Check.Fails(() => item.MoveTo(SeasonalBenchmarkGroupStatus.Created), "move");
    }

    public static void RunAll()
    {
        SeasonalBenchmarkGroupStartsInCreated();
        SeasonalBenchmarkGroupMovesToValidated();
        SeasonalBenchmarkGroupRejectsMovesFromRetired();
    }
}
