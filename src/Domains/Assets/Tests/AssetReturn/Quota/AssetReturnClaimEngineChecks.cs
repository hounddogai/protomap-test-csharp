namespace Acme.Workforce.Assets.Tests.AssetReturn.Quota;

using Acme.Workforce.Assets.AssetReturn.Quota;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class AssetReturnClaimEngineChecks
{
    public static void AssetReturnClaimEngineKeepsResultsWithinBounds()
    {
        var engine = AssetReturnClaimEngine.DefaultAssetReturnClaimEngine(Flags.NewRegistry());
        var item = AssetReturnClaim.NewAssetReturnClaim("code-707", "label-816");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssetReturnClaimEngineKeepsResultsWithinBounds();
    }
}
