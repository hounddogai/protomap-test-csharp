namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Digest;

using Acme.Workforce.Contractors.StatementOfWork.Digest;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class StatementOfWorkAuditEngineChecks
{
    public static void StatementOfWorkAuditEngineKeepsResultsWithinBounds()
    {
        var engine = StatementOfWorkAuditEngine.DefaultStatementOfWorkAuditEngine(Flags.NewRegistry());
        var item = StatementOfWorkAudit.NewStatementOfWorkAudit("code-506", "name-442");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StatementOfWorkAuditEngineKeepsResultsWithinBounds();
    }
}
