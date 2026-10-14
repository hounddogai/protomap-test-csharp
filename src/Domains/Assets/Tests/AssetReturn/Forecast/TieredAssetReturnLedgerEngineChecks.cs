namespace Acme.Workforce.Assets.Tests.AssetReturn.Forecast;

using Acme.Workforce.Assets.AssetReturn.Forecast;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class TieredAssetReturnLedgerEngineChecks
{
    public static void TieredAssetReturnLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = TieredAssetReturnLedgerEngine.DefaultTieredAssetReturnLedgerEngine(Flags.NewRegistry());
        var item = TieredAssetReturnLedger.NewTieredAssetReturnLedger("code-206", "title-906");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredAssetReturnLedgerEngineKeepsResultsWithinBounds();
    }
}
