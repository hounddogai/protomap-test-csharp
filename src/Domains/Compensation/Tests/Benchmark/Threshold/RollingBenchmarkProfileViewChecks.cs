namespace Acme.Workforce.Compensation.Tests.Benchmark.Threshold;

using Acme.Workforce.Compensation.Benchmark.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class RollingBenchmarkProfileViewChecks
{
    public static void RollingBenchmarkProfileViewShowsTheStatusInCapitals()
    {
        var item = RollingBenchmarkProfile.NewRollingBenchmarkProfile("code-486", "title-65");
        var view = RollingBenchmarkProfileView.ToRollingBenchmarkProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingBenchmarkProfileView.ToRollingBenchmarkProfileViews(new List<RollingBenchmarkProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingBenchmarkProfileViewShowsTheStatusInCapitals();
    }
}
