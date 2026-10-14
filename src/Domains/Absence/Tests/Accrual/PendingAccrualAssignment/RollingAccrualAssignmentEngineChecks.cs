namespace Acme.Workforce.Absence.Tests.Accrual.PendingAccrualAssignment;

using Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;

/// <summary>Checks of pending accrual assignment code, run by the project's runner.</summary>
public static class RollingAccrualAssignmentEngineChecks
{
    public static void RollingAccrualAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = RollingAccrualAssignmentEngine.DefaultRollingAccrualAssignmentEngine();
        var item = RollingAccrualAssignment.NewRollingAccrualAssignment("code-491", "title-549");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingAccrualAssignmentEngineKeepsResultsWithinBounds();
    }
}
