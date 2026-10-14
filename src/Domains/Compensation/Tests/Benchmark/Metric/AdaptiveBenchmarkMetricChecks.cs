namespace Acme.Workforce.Compensation.Tests.Benchmark.Metric;

using Acme.Workforce.Compensation.Benchmark.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class AdaptiveBenchmarkMetricChecks
{
    public static void AdaptiveBenchmarkMetricStartsInOpen()
    {
        var item = AdaptiveBenchmarkMetric.NewAdaptiveBenchmarkMetric("code-42", "label-686");
        Check.Equal(item.Status, AdaptiveBenchmarkMetricStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveBenchmarkMetricMovesToReopened()
    {
        var item = AdaptiveBenchmarkMetric.NewAdaptiveBenchmarkMetric("code-42", "label-686");
        try
        {
            item.MoveTo(AdaptiveBenchmarkMetricStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveBenchmarkMetricStatus.Reopened, "status");
    }

    public static void AdaptiveBenchmarkMetricRejectsMovesFromArchived()
    {
        var item = AdaptiveBenchmarkMetric.NewAdaptiveBenchmarkMetric("code-42", "label-686");
        item.Status = AdaptiveBenchmarkMetricStatus.Archived;
        Check.Fails(() => item.MoveTo(AdaptiveBenchmarkMetricStatus.Open), "move");
    }

    public static void RunAll()
    {
        AdaptiveBenchmarkMetricStartsInOpen();
        AdaptiveBenchmarkMetricMovesToReopened();
        AdaptiveBenchmarkMetricRejectsMovesFromArchived();
    }
}
