namespace Acme.Workforce.Assets.Tests.Warranty.Ledger;

using Acme.Workforce.Assets.Warranty.Ledger;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class TieredWarrantyTierEngineChecks
{
    public static void TieredWarrantyTierEngineKeepsResultsWithinBounds()
    {
        var engine = TieredWarrantyTierEngine.DefaultTieredWarrantyTierEngine(Flags.NewRegistry());
        var item = TieredWarrantyTier.NewTieredWarrantyTier("code-219", "label-342");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredWarrantyTierEngineKeepsResultsWithinBounds();
    }
}
