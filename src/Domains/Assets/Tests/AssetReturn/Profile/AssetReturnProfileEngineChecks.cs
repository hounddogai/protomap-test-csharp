namespace Acme.Workforce.Assets.Tests.AssetReturn.Profile;

using Acme.Workforce.Assets.AssetReturn.Profile;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class AssetReturnProfileEngineChecks
{
    public static void AssetReturnProfileEngineKeepsResultsWithinBounds()
    {
        var engine = AssetReturnProfileEngine.DefaultAssetReturnProfileEngine(Flags.NewRegistry());
        var item = AssetReturnProfile.NewAssetReturnProfile("code-443", "title-797");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssetReturnProfileEngineKeepsResultsWithinBounds();
    }
}
