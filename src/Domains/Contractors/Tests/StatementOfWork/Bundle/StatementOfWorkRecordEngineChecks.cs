namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Bundle;

using Acme.Workforce.Contractors.StatementOfWork.Bundle;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class StatementOfWorkRecordEngineChecks
{
    public static void StatementOfWorkRecordEngineKeepsResultsWithinBounds()
    {
        var engine = StatementOfWorkRecordEngine.DefaultStatementOfWorkRecordEngine(Flags.NewRegistry());
        var item = StatementOfWorkRecord.NewStatementOfWorkRecord("code-4", "name-63");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StatementOfWorkRecordEngineKeepsResultsWithinBounds();
    }
}
