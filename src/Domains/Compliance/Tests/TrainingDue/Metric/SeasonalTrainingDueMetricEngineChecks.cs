namespace Acme.Workforce.Compliance.Tests.TrainingDue.Metric;

using Acme.Workforce.Compliance.TrainingDue.Metric;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class SeasonalTrainingDueMetricEngineChecks
{
    public static void SeasonalTrainingDueMetricEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalTrainingDueMetricEngine.DefaultSeasonalTrainingDueMetricEngine(Flags.NewRegistry());
        var item = SeasonalTrainingDueMetric.NewSeasonalTrainingDueMetric("code-564", "title-56");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalTrainingDueMetricEngineKeepsResultsWithinBounds();
    }
}
