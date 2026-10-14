namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkSummary;

using Acme.Workforce.Compensation.Benchmark.BenchmarkSummary;

/// <summary>Checks of benchmark summary code, run by the project's runner.</summary>
public static class ManualBenchmarkAllocationViewChecks
{
    public static void ManualBenchmarkAllocationViewShowsTheStatusInCapitals()
    {
        var item = ManualBenchmarkAllocation.NewManualBenchmarkAllocation("code-36", "label-351");
        var view = ManualBenchmarkAllocationView.ToManualBenchmarkAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualBenchmarkAllocationView.ToManualBenchmarkAllocationViews(new List<ManualBenchmarkAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualBenchmarkAllocationViewShowsTheStatusInCapitals();
    }
}
