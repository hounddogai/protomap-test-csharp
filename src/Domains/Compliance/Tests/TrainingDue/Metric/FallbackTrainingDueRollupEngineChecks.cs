namespace Acme.Workforce.Compliance.Tests.TrainingDue.Metric;

using Acme.Workforce.Compliance.TrainingDue.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class FallbackTrainingDueRollupEngineChecks
{
    public static void FallbackTrainingDueRollupEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackTrainingDueRollupEngine.DefaultFallbackTrainingDueRollupEngine();
        var item = FallbackTrainingDueRollup.NewFallbackTrainingDueRollup("code-850", "title-688");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackTrainingDueRollupEngineKeepsResultsWithinBounds();
    }
}
