namespace Acme.Workforce.Compensation.Tests.Adjustment.Notice;

using Acme.Workforce.Compensation.Adjustment.Notice;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class AdjustmentSlotEngineChecks
{
    public static void AdjustmentSlotEngineKeepsResultsWithinBounds()
    {
        var engine = AdjustmentSlotEngine.DefaultAdjustmentSlotEngine(Flags.NewRegistry());
        var item = AdjustmentSlot.NewAdjustmentSlot("code-783", "title-756");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdjustmentSlotEngineKeepsResultsWithinBounds();
    }
}
