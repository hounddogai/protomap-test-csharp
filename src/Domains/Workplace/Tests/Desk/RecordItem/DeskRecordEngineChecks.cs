namespace Acme.Workforce.Workplace.Tests.Desk.RecordItem;

using Acme.Workforce.Workplace.Desk.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class DeskRecordEngineChecks
{
    public static void DeskRecordEngineKeepsResultsWithinBounds()
    {
        var engine = DeskRecordEngine.DefaultDeskRecordEngine();
        var item = DeskRecord.NewDeskRecord("code-781", "title-197");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DeskRecordEngineKeepsResultsWithinBounds();
    }
}
