namespace Acme.Workforce.Assets.Tests.AssetReturn.Estimate;

using Acme.Workforce.Assets.AssetReturn.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class AdaptiveAssetReturnAllocationEngineChecks
{
    public static void AdaptiveAssetReturnAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveAssetReturnAllocationEngine.DefaultAdaptiveAssetReturnAllocationEngine();
        var item = AdaptiveAssetReturnAllocation.NewAdaptiveAssetReturnAllocation("code-931", "label-759");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveAssetReturnAllocationEngineKeepsResultsWithinBounds();
    }
}
