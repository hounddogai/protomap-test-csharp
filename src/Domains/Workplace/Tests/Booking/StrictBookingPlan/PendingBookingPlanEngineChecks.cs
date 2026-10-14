namespace Acme.Workforce.Workplace.Tests.Booking.StrictBookingPlan;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Booking.StrictBookingPlan;

/// <summary>Checks of strict booking plan code, run by the project's runner.</summary>
public static class PendingBookingPlanEngineChecks
{
    public static void PendingBookingPlanEngineKeepsResultsWithinBounds()
    {
        var engine = PendingBookingPlanEngine.DefaultPendingBookingPlanEngine(Flags.NewRegistry());
        var item = PendingBookingPlan.NewPendingBookingPlan("code-998", "label-164");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingBookingPlanEngineKeepsResultsWithinBounds();
    }
}
