namespace Acme.Workforce.Talent.Tests.Pool.Adjustment;

using Acme.Workforce.Talent.Pool.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class PoolRecordEngineChecks
{
    public static void PoolRecordEngineKeepsResultsWithinBounds()
    {
        var engine = PoolRecordEngine.DefaultPoolRecordEngine();
        var item = PoolRecord.NewPoolRecord("code-705", "title-184");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PoolRecordEngineKeepsResultsWithinBounds();
    }
}
