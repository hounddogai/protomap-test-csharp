namespace Acme.Workforce.Workplace.Tests.Booking.Group;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Booking.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class CappedBookingTargetEngineChecks
{
    public static void CappedBookingTargetEngineKeepsResultsWithinBounds()
    {
        var engine = CappedBookingTargetEngine.DefaultCappedBookingTargetEngine(Flags.NewRegistry());
        var item = CappedBookingTarget.NewCappedBookingTarget("code-368", "name-196");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedBookingTargetEngineKeepsResultsWithinBounds();
    }
}
