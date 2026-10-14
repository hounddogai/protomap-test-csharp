namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkAudit;

using Acme.Workforce.Compensation.Benchmark.BenchmarkAudit;

/// <summary>Checks of benchmark audit code, run by the project's runner.</summary>
public static class StrictBenchmarkAuditViewChecks
{
    public static void StrictBenchmarkAuditViewShowsTheStatusInCapitals()
    {
        var item = StrictBenchmarkAudit.NewStrictBenchmarkAudit("code-943", "label-720");
        var view = StrictBenchmarkAuditView.ToStrictBenchmarkAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictBenchmarkAuditView.ToStrictBenchmarkAuditViews(new List<StrictBenchmarkAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictBenchmarkAuditViewShowsTheStatusInCapitals();
    }
}
