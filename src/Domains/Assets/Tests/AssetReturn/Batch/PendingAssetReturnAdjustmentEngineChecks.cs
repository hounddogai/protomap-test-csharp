namespace Acme.Workforce.Assets.Tests.AssetReturn.Batch;

using Acme.Workforce.Assets.AssetReturn.Batch;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PendingAssetReturnAdjustmentEngineChecks
{
    public static void PendingAssetReturnAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = PendingAssetReturnAdjustmentEngine.DefaultPendingAssetReturnAdjustmentEngine(Flags.NewRegistry());
        var item = PendingAssetReturnAdjustment.NewPendingAssetReturnAdjustment("code-331", "title-474");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingAssetReturnAdjustmentEngineKeepsResultsWithinBounds();
    }
}
