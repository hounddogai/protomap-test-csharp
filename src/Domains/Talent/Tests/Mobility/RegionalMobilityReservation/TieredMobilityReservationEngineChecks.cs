namespace Acme.Workforce.Talent.Tests.Mobility.RegionalMobilityReservation;

using Acme.Workforce.Talent.Mobility.RegionalMobilityReservation;

/// <summary>Checks of regional mobility reservation code, run by the project's runner.</summary>
public static class TieredMobilityReservationEngineChecks
{
    public static void TieredMobilityReservationEngineKeepsResultsWithinBounds()
    {
        var engine = TieredMobilityReservationEngine.DefaultTieredMobilityReservationEngine();
        var item = TieredMobilityReservation.NewTieredMobilityReservation("code-76", "name-656");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredMobilityReservationEngineKeepsResultsWithinBounds();
    }
}
