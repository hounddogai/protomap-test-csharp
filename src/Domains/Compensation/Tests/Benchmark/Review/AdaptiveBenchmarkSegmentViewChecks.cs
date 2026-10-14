namespace Acme.Workforce.Compensation.Tests.Benchmark.Review;

using Acme.Workforce.Compensation.Benchmark.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class AdaptiveBenchmarkSegmentViewChecks
{
    public static void AdaptiveBenchmarkSegmentViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveBenchmarkSegment.NewAdaptiveBenchmarkSegment("code-947", "name-313");
        var view = AdaptiveBenchmarkSegmentView.ToAdaptiveBenchmarkSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveBenchmarkSegmentView.ToAdaptiveBenchmarkSegmentViews(new List<AdaptiveBenchmarkSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveBenchmarkSegmentViewShowsTheStatusInCapitals();
    }
}
