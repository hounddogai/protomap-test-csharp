namespace Acme.Workforce.Compliance.Tests.Audit.Assignment;

using Acme.Workforce.Compliance.Audit.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class AuditAssignmentEngineChecks
{
    public static void AuditAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = AuditAssignmentEngine.DefaultAuditAssignmentEngine();
        var item = AuditAssignment.NewAuditAssignment("code-83", "name-795");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AuditAssignmentEngineKeepsResultsWithinBounds();
    }
}
