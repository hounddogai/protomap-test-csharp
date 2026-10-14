namespace Acme.Workforce.Compensation.Tests.BonusPool.Policy;

using Acme.Workforce.Compensation.BonusPool.Policy;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class BonusPoolCycleEngineChecks
{
    public static void BonusPoolCycleEngineKeepsResultsWithinBounds()
    {
        var engine = BonusPoolCycleEngine.DefaultBonusPoolCycleEngine(Flags.NewRegistry());
        var item = BonusPoolCycle.NewBonusPoolCycle("code-989", "label-238");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BonusPoolCycleEngineKeepsResultsWithinBounds();
    }
}
