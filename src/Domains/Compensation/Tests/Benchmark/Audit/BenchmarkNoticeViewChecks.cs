namespace Acme.Workforce.Compensation.Tests.Benchmark.Audit;

using Acme.Workforce.Compensation.Benchmark.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class BenchmarkNoticeViewChecks
{
    public static void BenchmarkNoticeViewShowsTheStatusInCapitals()
    {
        var item = BenchmarkNotice.NewBenchmarkNotice("code-470", "title-639");
        var view = BenchmarkNoticeView.ToBenchmarkNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BenchmarkNoticeView.ToBenchmarkNoticeViews(new List<BenchmarkNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BenchmarkNoticeViewShowsTheStatusInCapitals();
    }
}
