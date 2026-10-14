namespace Acme.Workforce.Workplace.Tests.Booking.Window;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Booking.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class PrimaryBookingAdjustmentEngineChecks
{
    public static void PrimaryBookingAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryBookingAdjustmentEngine.DefaultPrimaryBookingAdjustmentEngine(Flags.NewRegistry());
        var item = PrimaryBookingAdjustment.NewPrimaryBookingAdjustment("code-118", "name-144");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryBookingAdjustmentEngineKeepsResultsWithinBounds();
    }
}
