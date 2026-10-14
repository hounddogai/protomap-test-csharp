namespace Acme.Workforce.Compensation.Tests.BonusPool.Allocation;

using Acme.Workforce.Compensation.BonusPool.Allocation;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class BonusPoolAllocationEngineChecks
{
    public static void BonusPoolAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = BonusPoolAllocationEngine.DefaultBonusPoolAllocationEngine(Flags.NewRegistry());
        var item = BonusPoolAllocation.NewBonusPoolAllocation("code-496", "name-102");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BonusPoolAllocationEngineKeepsResultsWithinBounds();
    }
}
