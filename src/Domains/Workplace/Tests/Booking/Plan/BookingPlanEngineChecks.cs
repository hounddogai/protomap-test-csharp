namespace Acme.Workforce.Workplace.Tests.Booking.Plan;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Booking.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class BookingPlanEngineChecks
{
    public static void BookingPlanEngineKeepsResultsWithinBounds()
    {
        var engine = BookingPlanEngine.DefaultBookingPlanEngine(Flags.NewRegistry());
        var item = BookingPlan.NewBookingPlan("code-618", "name-584");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BookingPlanEngineKeepsResultsWithinBounds();
    }
}
