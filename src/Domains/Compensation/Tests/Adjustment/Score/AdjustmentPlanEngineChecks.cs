namespace Acme.Workforce.Compensation.Tests.Adjustment.Score;

using Acme.Workforce.Compensation.Adjustment.Score;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class AdjustmentPlanEngineChecks
{
    public static void AdjustmentPlanEngineKeepsResultsWithinBounds()
    {
        var engine = AdjustmentPlanEngine.DefaultAdjustmentPlanEngine(Flags.NewRegistry());
        var item = AdjustmentPlan.NewAdjustmentPlan("code-836", "label-183");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdjustmentPlanEngineKeepsResultsWithinBounds();
    }
}
