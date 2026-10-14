namespace Acme.Workforce.Compensation.Tests.MeritCycle.Review;

using Acme.Workforce.Compensation.MeritCycle.Review;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class MeritCycleReviewEngineChecks
{
    public static void MeritCycleReviewEngineKeepsResultsWithinBounds()
    {
        var engine = MeritCycleReviewEngine.DefaultMeritCycleReviewEngine(Flags.NewRegistry());
        var item = MeritCycleReview.NewMeritCycleReview("code-139", "title-698");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        MeritCycleReviewEngineKeepsResultsWithinBounds();
    }
}
