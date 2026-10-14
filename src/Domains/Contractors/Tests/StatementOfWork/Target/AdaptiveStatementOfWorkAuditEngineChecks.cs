namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Target;

using Acme.Workforce.Contractors.StatementOfWork.Target;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class AdaptiveStatementOfWorkAuditEngineChecks
{
    public static void AdaptiveStatementOfWorkAuditEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveStatementOfWorkAuditEngine.DefaultAdaptiveStatementOfWorkAuditEngine(Flags.NewRegistry());
        var item = AdaptiveStatementOfWorkAudit.NewAdaptiveStatementOfWorkAudit("code-259", "label-775");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveStatementOfWorkAuditEngineKeepsResultsWithinBounds();
    }
}
