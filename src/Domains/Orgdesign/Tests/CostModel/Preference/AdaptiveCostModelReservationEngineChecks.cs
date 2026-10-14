namespace Acme.Workforce.Orgdesign.Tests.CostModel.Preference;

using Acme.Workforce.Orgdesign.CostModel.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class AdaptiveCostModelReservationEngineChecks
{
    public static void AdaptiveCostModelReservationEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveCostModelReservationEngine.DefaultAdaptiveCostModelReservationEngine();
        var item = AdaptiveCostModelReservation.NewAdaptiveCostModelReservation("code-735", "name-977");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveCostModelReservationEngineKeepsResultsWithinBounds();
    }
}
