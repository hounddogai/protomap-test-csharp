namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyAdjustment;

using Acme.Workforce.Assets.Warranty.WarrantyAdjustment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of warranty adjustment code, run by the project's runner.</summary>
public static class LenientWarrantyAdjustmentEngineChecks
{
    public static void LenientWarrantyAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = LenientWarrantyAdjustmentEngine.DefaultLenientWarrantyAdjustmentEngine(Flags.NewRegistry());
        var item = LenientWarrantyAdjustment.NewLenientWarrantyAdjustment("code-106", "title-48");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientWarrantyAdjustmentEngineKeepsResultsWithinBounds();
    }
}
