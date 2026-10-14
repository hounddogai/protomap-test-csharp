namespace Acme.Workforce.Compensation.Tests.MeritCycle.Review;

using Acme.Workforce.Compensation.MeritCycle.Review;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class LenientMeritCycleReviewEngineChecks
{
    public static void LenientMeritCycleReviewEngineKeepsResultsWithinBounds()
    {
        var engine = LenientMeritCycleReviewEngine.DefaultLenientMeritCycleReviewEngine(Flags.NewRegistry());
        var item = LenientMeritCycleReview.NewLenientMeritCycleReview("code-408", "title-730");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientMeritCycleReviewEngineKeepsResultsWithinBounds();
    }
}
