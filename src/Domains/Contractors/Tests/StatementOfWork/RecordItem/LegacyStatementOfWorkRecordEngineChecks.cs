namespace Acme.Workforce.Contractors.Tests.StatementOfWork.RecordItem;

using Acme.Workforce.Contractors.StatementOfWork.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LegacyStatementOfWorkRecordEngineChecks
{
    public static void LegacyStatementOfWorkRecordEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyStatementOfWorkRecordEngine.DefaultLegacyStatementOfWorkRecordEngine();
        var item = LegacyStatementOfWorkRecord.NewLegacyStatementOfWorkRecord("code-14", "name-98");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyStatementOfWorkRecordEngineKeepsResultsWithinBounds();
    }
}
