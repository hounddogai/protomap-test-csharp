namespace Acme.Workforce.Workplace.Tests.AccessBadge.Threshold;

using Acme.Workforce.Workplace.AccessBadge.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class AccessBadgeReservationEngineChecks
{
    public static void AccessBadgeReservationEngineKeepsResultsWithinBounds()
    {
        var engine = AccessBadgeReservationEngine.DefaultAccessBadgeReservationEngine();
        var item = AccessBadgeReservation.NewAccessBadgeReservation("code-917", "label-786");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccessBadgeReservationEngineKeepsResultsWithinBounds();
    }
}
