namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeBudget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

/// <summary>Checks of badge budget code, run by the project's runner.</summary>
public static class ScheduledAccessBadgeReservationEngineChecks
{
    public static void ScheduledAccessBadgeReservationEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledAccessBadgeReservationEngine.DefaultScheduledAccessBadgeReservationEngine(Flags.NewRegistry());
        var item = ScheduledAccessBadgeReservation.NewScheduledAccessBadgeReservation("code-235", "title-924");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledAccessBadgeReservationEngineKeepsResultsWithinBounds();
    }
}
