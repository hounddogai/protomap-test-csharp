namespace Acme.Workforce.Payrollops.Tests.PayRun.RunRecord;

using Acme.Workforce.Payrollops.PayRun.RunRecord;

/// <summary>Checks of run record code, run by the project's runner.</summary>
public static class CappedPayRunRecordEngineChecks
{
    public static void CappedPayRunRecordEngineKeepsResultsWithinBounds()
    {
        var engine = CappedPayRunRecordEngine.DefaultCappedPayRunRecordEngine();
        var item = CappedPayRunRecord.NewCappedPayRunRecord("code-986", "name-43");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedPayRunRecordEngineKeepsResultsWithinBounds();
    }
}
