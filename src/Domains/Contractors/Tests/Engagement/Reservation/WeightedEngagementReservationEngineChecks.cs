namespace Acme.Workforce.Contractors.Tests.Engagement.Reservation;

using Acme.Workforce.Contractors.Engagement.Reservation;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class WeightedEngagementReservationEngineChecks
{
    public static void WeightedEngagementReservationEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedEngagementReservationEngine.DefaultWeightedEngagementReservationEngine(Flags.NewRegistry());
        var item = WeightedEngagementReservation.NewWeightedEngagementReservation("code-721", "label-107");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedEngagementReservationEngineKeepsResultsWithinBounds();
    }
}
