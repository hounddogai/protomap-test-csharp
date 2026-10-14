namespace Acme.Workforce.Assets.Tests.AssetReturn.Assignment;

using Acme.Workforce.Assets.AssetReturn.Assignment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class AssetReturnPolicyEngineChecks
{
    public static void AssetReturnPolicyEngineKeepsResultsWithinBounds()
    {
        var engine = AssetReturnPolicyEngine.DefaultAssetReturnPolicyEngine(Flags.NewRegistry());
        var item = AssetReturnPolicy.NewAssetReturnPolicy("code-814", "label-922");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssetReturnPolicyEngineKeepsResultsWithinBounds();
    }
}
