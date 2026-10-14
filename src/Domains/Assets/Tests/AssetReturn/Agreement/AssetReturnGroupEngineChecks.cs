namespace Acme.Workforce.Assets.Tests.AssetReturn.Agreement;

using Acme.Workforce.Assets.AssetReturn.Agreement;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class AssetReturnGroupEngineChecks
{
    public static void AssetReturnGroupEngineKeepsResultsWithinBounds()
    {
        var engine = AssetReturnGroupEngine.DefaultAssetReturnGroupEngine(Flags.NewRegistry());
        var item = AssetReturnGroup.NewAssetReturnGroup("code-724", "title-470");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssetReturnGroupEngineKeepsResultsWithinBounds();
    }
}
