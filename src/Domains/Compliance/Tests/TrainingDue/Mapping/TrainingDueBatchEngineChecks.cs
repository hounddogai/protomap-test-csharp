namespace Acme.Workforce.Compliance.Tests.TrainingDue.Mapping;

using Acme.Workforce.Compliance.TrainingDue.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class TrainingDueBatchEngineChecks
{
    public static void TrainingDueBatchEngineKeepsResultsWithinBounds()
    {
        var engine = TrainingDueBatchEngine.DefaultTrainingDueBatchEngine();
        var item = TrainingDueBatch.NewTrainingDueBatch("code-483", "name-286");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TrainingDueBatchEngineKeepsResultsWithinBounds();
    }
}
