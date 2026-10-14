namespace Acme.Workforce.Compensation.Tests.MeritCycle.Review;

using Acme.Workforce.Compensation.MeritCycle.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class MeritCycleAuditEngineChecks
{
    public static void MeritCycleAuditEngineKeepsResultsWithinBounds()
    {
        var engine = MeritCycleAuditEngine.DefaultMeritCycleAuditEngine();
        var item = MeritCycleAudit.NewMeritCycleAudit("code-281", "name-494");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        MeritCycleAuditEngineKeepsResultsWithinBounds();
    }
}
