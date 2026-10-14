namespace Acme.Workforce.Compensation.Tests.Benchmark.Review;

using Acme.Workforce.Compensation.Benchmark.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class AdaptiveBenchmarkSegmentChecks
{
    public static void AdaptiveBenchmarkSegmentStartsInOpen()
    {
        var item = AdaptiveBenchmarkSegment.NewAdaptiveBenchmarkSegment("code-947", "name-313");
        Check.Equal(item.Status, AdaptiveBenchmarkSegmentStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveBenchmarkSegmentMovesToInReview()
    {
        var item = AdaptiveBenchmarkSegment.NewAdaptiveBenchmarkSegment("code-947", "name-313");
        try
        {
            item.MoveTo(AdaptiveBenchmarkSegmentStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveBenchmarkSegmentStatus.InReview, "status");
    }

    public static void AdaptiveBenchmarkSegmentRejectsMovesFromArchived()
    {
        var item = AdaptiveBenchmarkSegment.NewAdaptiveBenchmarkSegment("code-947", "name-313");
        item.Status = AdaptiveBenchmarkSegmentStatus.Archived;
        Check.Fails(() => item.MoveTo(AdaptiveBenchmarkSegmentStatus.Open), "move");
    }

    public static void RunAll()
    {
        AdaptiveBenchmarkSegmentStartsInOpen();
        AdaptiveBenchmarkSegmentMovesToInReview();
        AdaptiveBenchmarkSegmentRejectsMovesFromArchived();
    }
}
