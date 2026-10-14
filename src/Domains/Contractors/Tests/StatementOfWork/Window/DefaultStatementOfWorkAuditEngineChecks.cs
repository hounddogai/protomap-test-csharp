namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Window;

using Acme.Workforce.Contractors.StatementOfWork.Window;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class DefaultStatementOfWorkAuditEngineChecks
{
    public static void DefaultStatementOfWorkAuditEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultStatementOfWorkAuditEngine.DefaultDefaultStatementOfWorkAuditEngine(Flags.NewRegistry());
        var item = DefaultStatementOfWorkAudit.NewDefaultStatementOfWorkAudit("code-643", "name-387");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultStatementOfWorkAuditEngineKeepsResultsWithinBounds();
    }
}
