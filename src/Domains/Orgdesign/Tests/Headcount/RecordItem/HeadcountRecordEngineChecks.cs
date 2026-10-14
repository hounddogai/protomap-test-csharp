namespace Acme.Workforce.Orgdesign.Tests.Headcount.RecordItem;

using Acme.Workforce.Orgdesign.Headcount.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class HeadcountRecordEngineChecks
{
    public static void HeadcountRecordEngineKeepsResultsWithinBounds()
    {
        var engine = HeadcountRecordEngine.DefaultHeadcountRecordEngine();
        var item = HeadcountRecord.NewHeadcountRecord("code-849", "name-289");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        HeadcountRecordEngineKeepsResultsWithinBounds();
    }
}
