namespace Acme.Workforce.Payrollops.Tests.Deduction.Profile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Deduction.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class GlobalDeductionReservationEngineChecks
{
    public static void GlobalDeductionReservationEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalDeductionReservationEngine.DefaultGlobalDeductionReservationEngine(Flags.NewRegistry());
        var item = GlobalDeductionReservation.NewGlobalDeductionReservation("code-479", "title-418");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalDeductionReservationEngineKeepsResultsWithinBounds();
    }
}
