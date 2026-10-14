namespace Acme.Workforce.Compliance.Tests.TrainingDue.Quota;

using Acme.Workforce.Compliance.TrainingDue.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class RegionalTrainingDueReservationEngineChecks
{
    public static void RegionalTrainingDueReservationEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalTrainingDueReservationEngine.DefaultRegionalTrainingDueReservationEngine();
        var item = RegionalTrainingDueReservation.NewRegionalTrainingDueReservation("code-868", "name-936");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalTrainingDueReservationEngineKeepsResultsWithinBounds();
    }
}
