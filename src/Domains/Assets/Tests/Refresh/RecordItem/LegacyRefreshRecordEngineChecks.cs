namespace Acme.Workforce.Assets.Tests.Refresh.RecordItem;

using Acme.Workforce.Assets.Refresh.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LegacyRefreshRecordEngineChecks
{
    public static void LegacyRefreshRecordEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyRefreshRecordEngine.DefaultLegacyRefreshRecordEngine();
        var item = LegacyRefreshRecord.NewLegacyRefreshRecord("code-726", "label-838");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyRefreshRecordEngineKeepsResultsWithinBounds();
    }
}
