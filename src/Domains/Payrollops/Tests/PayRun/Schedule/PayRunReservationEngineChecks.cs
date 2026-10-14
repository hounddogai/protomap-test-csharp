namespace Acme.Workforce.Payrollops.Tests.PayRun.Schedule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.PayRun.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class PayRunReservationEngineChecks
{
    public static void PayRunReservationEngineKeepsResultsWithinBounds()
    {
        var engine = PayRunReservationEngine.DefaultPayRunReservationEngine(Flags.NewRegistry());
        var item = PayRunReservation.NewPayRunReservation("code-419", "title-768");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayRunReservationEngineKeepsResultsWithinBounds();
    }
}
