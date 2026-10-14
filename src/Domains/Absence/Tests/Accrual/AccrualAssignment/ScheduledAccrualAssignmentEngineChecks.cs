namespace Acme.Workforce.Absence.Tests.Accrual.AccrualAssignment;

using Acme.Workforce.Absence.Accrual.AccrualAssignment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of accrual assignment code, run by the project's runner.</summary>
public static class ScheduledAccrualAssignmentEngineChecks
{
    public static void ScheduledAccrualAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledAccrualAssignmentEngine.DefaultScheduledAccrualAssignmentEngine(Flags.NewRegistry());
        var item = ScheduledAccrualAssignment.NewScheduledAccrualAssignment("code-768", "label-996");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledAccrualAssignmentEngineKeepsResultsWithinBounds();
    }
}
