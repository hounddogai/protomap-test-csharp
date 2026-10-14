namespace Acme.Workforce.Compensation.Tests.BonusPool.Rollup;

using Acme.Workforce.Compensation.BonusPool.Rollup;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class BonusPoolPlanEngineChecks
{
    public static void BonusPoolPlanEngineKeepsResultsWithinBounds()
    {
        var engine = BonusPoolPlanEngine.DefaultBonusPoolPlanEngine(Flags.NewRegistry());
        var item = BonusPoolPlan.NewBonusPoolPlan("code-46", "name-929");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BonusPoolPlanEngineKeepsResultsWithinBounds();
    }
}
