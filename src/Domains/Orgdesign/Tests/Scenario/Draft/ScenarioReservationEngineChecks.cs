namespace Acme.Workforce.Orgdesign.Tests.Scenario.Draft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Scenario.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class ScenarioReservationEngineChecks
{
    public static void ScenarioReservationEngineKeepsResultsWithinBounds()
    {
        var engine = ScenarioReservationEngine.DefaultScenarioReservationEngine(Flags.NewRegistry());
        var item = ScenarioReservation.NewScenarioReservation("code-546", "title-817");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScenarioReservationEngineKeepsResultsWithinBounds();
    }
}
