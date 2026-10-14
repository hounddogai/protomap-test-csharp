namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkAudit;

using Acme.Workforce.Compensation.Benchmark.BenchmarkAudit;

/// <summary>Checks of benchmark audit code, run by the project's runner.</summary>
public static class BenchmarkScheduleViewChecks
{
    public static void BenchmarkScheduleViewShowsTheStatusInCapitals()
    {
        var item = BenchmarkSchedule.NewBenchmarkSchedule("code-543", "label-757");
        var view = BenchmarkScheduleView.ToBenchmarkScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BenchmarkScheduleView.ToBenchmarkScheduleViews(new List<BenchmarkSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BenchmarkScheduleViewShowsTheStatusInCapitals();
    }
}
