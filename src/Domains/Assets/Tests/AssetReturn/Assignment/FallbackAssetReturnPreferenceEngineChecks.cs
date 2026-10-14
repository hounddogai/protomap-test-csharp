namespace Acme.Workforce.Assets.Tests.AssetReturn.Assignment;

using Acme.Workforce.Assets.AssetReturn.Assignment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class FallbackAssetReturnPreferenceEngineChecks
{
    public static void FallbackAssetReturnPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackAssetReturnPreferenceEngine.DefaultFallbackAssetReturnPreferenceEngine(Flags.NewRegistry());
        var item = FallbackAssetReturnPreference.NewFallbackAssetReturnPreference("code-421", "label-508");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackAssetReturnPreferenceEngineKeepsResultsWithinBounds();
    }
}
