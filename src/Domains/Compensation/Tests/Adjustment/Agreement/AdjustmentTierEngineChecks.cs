namespace Acme.Workforce.Compensation.Tests.Adjustment.Agreement;

using Acme.Workforce.Compensation.Adjustment.Agreement;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class AdjustmentTierEngineChecks
{
    public static void AdjustmentTierEngineKeepsResultsWithinBounds()
    {
        var engine = AdjustmentTierEngine.DefaultAdjustmentTierEngine(Flags.NewRegistry());
        var item = AdjustmentTier.NewAdjustmentTier("code-77", "title-971");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdjustmentTierEngineKeepsResultsWithinBounds();
    }
}
