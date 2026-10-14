namespace Acme.Workforce.Contractors.Tests.Engagement.Manifest;

using Acme.Workforce.Contractors.Engagement.Manifest;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class EngagementReservationEngineChecks
{
    public static void EngagementReservationEngineKeepsResultsWithinBounds()
    {
        var engine = EngagementReservationEngine.DefaultEngagementReservationEngine(Flags.NewRegistry());
        var item = EngagementReservation.NewEngagementReservation("code-761", "title-287");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        EngagementReservationEngineKeepsResultsWithinBounds();
    }
}
