namespace Acme.Workforce.Compliance.Tests.TrainingDue.Schedule;

using Acme.Workforce.Compliance.TrainingDue.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class TrainingDueMetricEngineChecks
{
    public static void TrainingDueMetricEngineKeepsResultsWithinBounds()
    {
        var engine = TrainingDueMetricEngine.DefaultTrainingDueMetricEngine();
        var item = TrainingDueMetric.NewTrainingDueMetric("code-376", "title-902");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TrainingDueMetricEngineKeepsResultsWithinBounds();
    }
}
