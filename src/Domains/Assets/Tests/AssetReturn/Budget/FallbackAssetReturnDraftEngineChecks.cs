namespace Acme.Workforce.Assets.Tests.AssetReturn.Budget;

using Acme.Workforce.Assets.AssetReturn.Budget;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class FallbackAssetReturnDraftEngineChecks
{
    public static void FallbackAssetReturnDraftEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackAssetReturnDraftEngine.DefaultFallbackAssetReturnDraftEngine(Flags.NewRegistry());
        var item = FallbackAssetReturnDraft.NewFallbackAssetReturnDraft("code-492", "title-409");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackAssetReturnDraftEngineKeepsResultsWithinBounds();
    }
}
