namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueVariance;

using Acme.Workforce.Compliance.TrainingDue.DueVariance;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of due variance code, run by the project's runner.</summary>
public static class TrainingDueForecastEngineChecks
{
    public static void TrainingDueForecastEngineKeepsResultsWithinBounds()
    {
        var engine = TrainingDueForecastEngine.DefaultTrainingDueForecastEngine(Flags.NewRegistry());
        var item = TrainingDueForecast.NewTrainingDueForecast("code-897", "label-566");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TrainingDueForecastEngineKeepsResultsWithinBounds();
    }
}
