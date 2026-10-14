namespace Acme.Workforce.Workplace.Tests.Desk.DeskRecord;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Desk.DeskRecord;

/// <summary>Checks of desk record code, run by the project's runner.</summary>
public static class CappedDeskRecordEngineChecks
{
    public static void CappedDeskRecordEngineKeepsResultsWithinBounds()
    {
        var engine = CappedDeskRecordEngine.DefaultCappedDeskRecordEngine(Flags.NewRegistry());
        var item = CappedDeskRecord.NewCappedDeskRecord("code-331", "name-917");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedDeskRecordEngineKeepsResultsWithinBounds();
    }
}
