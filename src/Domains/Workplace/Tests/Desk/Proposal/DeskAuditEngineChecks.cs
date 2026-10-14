namespace Acme.Workforce.Workplace.Tests.Desk.Proposal;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Desk.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DeskAuditEngineChecks
{
    public static void DeskAuditEngineKeepsResultsWithinBounds()
    {
        var engine = DeskAuditEngine.DefaultDeskAuditEngine(Flags.NewRegistry());
        var item = DeskAudit.NewDeskAudit("code-610", "label-231");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DeskAuditEngineKeepsResultsWithinBounds();
    }
}
