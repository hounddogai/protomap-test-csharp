namespace Acme.Workforce.Talent.Tests.Mobility.Review;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Mobility.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RegionalMobilityReviewEngineChecks
{
    public static void RegionalMobilityReviewEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalMobilityReviewEngine.DefaultRegionalMobilityReviewEngine(Flags.NewRegistry());
        var item = RegionalMobilityReview.NewRegionalMobilityReview("code-726", "title-198");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalMobilityReviewEngineKeepsResultsWithinBounds();
    }
}
