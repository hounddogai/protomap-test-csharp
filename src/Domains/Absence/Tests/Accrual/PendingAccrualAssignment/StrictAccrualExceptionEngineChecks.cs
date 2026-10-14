namespace Acme.Workforce.Absence.Tests.Accrual.PendingAccrualAssignment;

using Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of pending accrual assignment code, run by the project's runner.</summary>
public static class StrictAccrualExceptionEngineChecks
{
    public static void StrictAccrualExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = StrictAccrualExceptionEngine.DefaultStrictAccrualExceptionEngine(Flags.NewRegistry());
        var item = StrictAccrualException.NewStrictAccrualException("code-951", "label-446");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictAccrualExceptionEngineKeepsResultsWithinBounds();
    }
}
