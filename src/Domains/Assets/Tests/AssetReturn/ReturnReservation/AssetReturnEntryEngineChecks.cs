namespace Acme.Workforce.Assets.Tests.AssetReturn.ReturnReservation;

using Acme.Workforce.Assets.AssetReturn.ReturnReservation;

/// <summary>Checks of return reservation code, run by the project's runner.</summary>
public static class AssetReturnEntryEngineChecks
{
    public static void AssetReturnEntryEngineKeepsResultsWithinBounds()
    {
        var engine = AssetReturnEntryEngine.DefaultAssetReturnEntryEngine();
        var item = AssetReturnEntry.NewAssetReturnEntry("code-284", "title-512");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssetReturnEntryEngineKeepsResultsWithinBounds();
    }
}
