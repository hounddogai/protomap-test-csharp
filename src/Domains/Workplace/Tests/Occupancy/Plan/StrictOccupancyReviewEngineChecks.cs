namespace Acme.Workforce.Workplace.Tests.Occupancy.Plan;

using Acme.Workforce.Workplace.Occupancy.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class StrictOccupancyReviewEngineChecks
{
    public static void StrictOccupancyReviewEngineKeepsResultsWithinBounds()
    {
        var engine = StrictOccupancyReviewEngine.DefaultStrictOccupancyReviewEngine();
        var item = StrictOccupancyReview.NewStrictOccupancyReview("code-869", "title-229");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictOccupancyReviewEngineKeepsResultsWithinBounds();
    }
}
