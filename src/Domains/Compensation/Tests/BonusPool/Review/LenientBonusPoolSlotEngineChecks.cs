namespace Acme.Workforce.Compensation.Tests.BonusPool.Review;

using Acme.Workforce.Compensation.BonusPool.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class LenientBonusPoolSlotEngineChecks
{
    public static void LenientBonusPoolSlotEngineKeepsResultsWithinBounds()
    {
        var engine = LenientBonusPoolSlotEngine.DefaultLenientBonusPoolSlotEngine();
        var item = LenientBonusPoolSlot.NewLenientBonusPoolSlot("code-203", "title-221");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientBonusPoolSlotEngineKeepsResultsWithinBounds();
    }
}
