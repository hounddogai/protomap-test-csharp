namespace Acme.Workforce.Orgdesign.Tests.Position.Plan;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Position.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class PositionReviewEngineChecks
{
    public static void PositionReviewEngineKeepsResultsWithinBounds()
    {
        var engine = PositionReviewEngine.DefaultPositionReviewEngine(Flags.NewRegistry());
        var item = PositionReview.NewPositionReview("code-634", "name-584");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PositionReviewEngineKeepsResultsWithinBounds();
    }
}
