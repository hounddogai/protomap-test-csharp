namespace Acme.Workforce.Absence.Tests.LeaveRequest.Snapshot;

using Acme.Workforce.Absence.LeaveRequest.Snapshot;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class LegacyLeaveRequestAuditEngineChecks
{
    public static void LegacyLeaveRequestAuditEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyLeaveRequestAuditEngine.DefaultLegacyLeaveRequestAuditEngine(Flags.NewRegistry());
        var item = LegacyLeaveRequestAudit.NewLegacyLeaveRequestAudit("code-997", "label-644");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyLeaveRequestAuditEngineKeepsResultsWithinBounds();
    }
}
