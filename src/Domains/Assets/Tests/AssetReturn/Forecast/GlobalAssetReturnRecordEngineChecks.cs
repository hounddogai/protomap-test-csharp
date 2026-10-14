namespace Acme.Workforce.Assets.Tests.AssetReturn.Forecast;

using Acme.Workforce.Assets.AssetReturn.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class GlobalAssetReturnRecordEngineChecks
{
    public static void GlobalAssetReturnRecordEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalAssetReturnRecordEngine.DefaultGlobalAssetReturnRecordEngine();
        var item = GlobalAssetReturnRecord.NewGlobalAssetReturnRecord("code-611", "name-658");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalAssetReturnRecordEngineKeepsResultsWithinBounds();
    }
}
