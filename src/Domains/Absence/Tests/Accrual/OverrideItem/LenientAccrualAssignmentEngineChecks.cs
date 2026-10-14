namespace Acme.Workforce.Absence.Tests.Accrual.OverrideItem;

using Acme.Workforce.Absence.Accrual.OverrideItem;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class LenientAccrualAssignmentEngineChecks
{
    public static void LenientAccrualAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = LenientAccrualAssignmentEngine.DefaultLenientAccrualAssignmentEngine(Flags.NewRegistry());
        var item = LenientAccrualAssignment.NewLenientAccrualAssignment("code-301", "label-784");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientAccrualAssignmentEngineKeepsResultsWithinBounds();
    }
}
