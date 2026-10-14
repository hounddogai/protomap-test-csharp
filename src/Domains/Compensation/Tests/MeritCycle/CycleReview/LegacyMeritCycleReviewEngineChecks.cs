namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleReview;

using Acme.Workforce.Compensation.MeritCycle.CycleReview;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of cycle review code, run by the project's runner.</summary>
public static class LegacyMeritCycleReviewEngineChecks
{
    public static void LegacyMeritCycleReviewEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyMeritCycleReviewEngine.DefaultLegacyMeritCycleReviewEngine(Flags.NewRegistry());
        var item = LegacyMeritCycleReview.NewLegacyMeritCycleReview("code-572", "name-40");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyMeritCycleReviewEngineKeepsResultsWithinBounds();
    }
}
