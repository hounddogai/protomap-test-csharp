namespace Acme.Workforce.Absence.Tests.Accrual.Assignment;

using Acme.Workforce.Absence.Accrual.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class AccrualAssignmentEngineChecks
{
    public static void AccrualAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = AccrualAssignmentEngine.DefaultAccrualAssignmentEngine();
        var item = AccrualAssignment.NewAccrualAssignment("code-753", "title-491");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccrualAssignmentEngineKeepsResultsWithinBounds();
    }
}
