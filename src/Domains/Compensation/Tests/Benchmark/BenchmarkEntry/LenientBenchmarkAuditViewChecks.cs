namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkEntry;

using Acme.Workforce.Compensation.Benchmark.BenchmarkEntry;

/// <summary>Checks of benchmark entry code, run by the project's runner.</summary>
public static class LenientBenchmarkAuditViewChecks
{
    public static void LenientBenchmarkAuditViewShowsTheStatusInCapitals()
    {
        var item = LenientBenchmarkAudit.NewLenientBenchmarkAudit("code-716", "title-308");
        var view = LenientBenchmarkAuditView.ToLenientBenchmarkAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientBenchmarkAuditView.ToLenientBenchmarkAuditViews(new List<LenientBenchmarkAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientBenchmarkAuditViewShowsTheStatusInCapitals();
    }
}
