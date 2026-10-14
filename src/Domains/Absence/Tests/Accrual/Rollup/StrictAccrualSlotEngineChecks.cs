namespace Acme.Workforce.Absence.Tests.Accrual.Rollup;

using Acme.Workforce.Absence.Accrual.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class StrictAccrualSlotEngineChecks
{
    public static void StrictAccrualSlotEngineKeepsResultsWithinBounds()
    {
        var engine = StrictAccrualSlotEngine.DefaultStrictAccrualSlotEngine();
        var item = StrictAccrualSlot.NewStrictAccrualSlot("code-762", "label-673");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictAccrualSlotEngineKeepsResultsWithinBounds();
    }
}
