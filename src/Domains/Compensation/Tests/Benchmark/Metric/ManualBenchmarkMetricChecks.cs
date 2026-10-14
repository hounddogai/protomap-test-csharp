namespace Acme.Workforce.Compensation.Tests.Benchmark.Metric;

using Acme.Workforce.Compensation.Benchmark.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ManualBenchmarkMetricChecks
{
    public static void ManualBenchmarkMetricStartsInProposed()
    {
        var item = ManualBenchmarkMetric.NewManualBenchmarkMetric("code-971", "label-834");
        Check.Equal(item.Status, ManualBenchmarkMetricStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualBenchmarkMetricMovesToApproved()
    {
        var item = ManualBenchmarkMetric.NewManualBenchmarkMetric("code-971", "label-834");
        try
        {
            item.MoveTo(ManualBenchmarkMetricStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualBenchmarkMetricStatus.Approved, "status");
    }

    public static void ManualBenchmarkMetricRejectsMovesFromWithdrawn()
    {
        var item = ManualBenchmarkMetric.NewManualBenchmarkMetric("code-971", "label-834");
        item.Status = ManualBenchmarkMetricStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ManualBenchmarkMetricStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ManualBenchmarkMetricStartsInProposed();
        ManualBenchmarkMetricMovesToApproved();
        ManualBenchmarkMetricRejectsMovesFromWithdrawn();
    }
}
