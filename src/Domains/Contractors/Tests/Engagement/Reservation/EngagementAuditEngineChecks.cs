namespace Acme.Workforce.Contractors.Tests.Engagement.Reservation;

using Acme.Workforce.Contractors.Engagement.Reservation;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class EngagementAuditEngineChecks
{
    public static void EngagementAuditEngineKeepsResultsWithinBounds()
    {
        var engine = EngagementAuditEngine.DefaultEngagementAuditEngine(Flags.NewRegistry());
        var item = EngagementAudit.NewEngagementAudit("code-396", "name-419");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        EngagementAuditEngineKeepsResultsWithinBounds();
    }
}
