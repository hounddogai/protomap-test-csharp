namespace Acme.Workforce.Absence.Tests.Balance.Summary;

using Acme.Workforce.Absence.Balance.Summary;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PendingBalanceAuditEngineChecks
{
    public static void PendingBalanceAuditEngineKeepsResultsWithinBounds()
    {
        var engine = PendingBalanceAuditEngine.DefaultPendingBalanceAuditEngine(Flags.NewRegistry());
        var item = PendingBalanceAudit.NewPendingBalanceAudit("code-329", "label-32");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingBalanceAuditEngineKeepsResultsWithinBounds();
    }
}
