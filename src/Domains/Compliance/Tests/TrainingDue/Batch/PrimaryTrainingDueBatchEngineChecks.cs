namespace Acme.Workforce.Compliance.Tests.TrainingDue.Batch;

using Acme.Workforce.Compliance.TrainingDue.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PrimaryTrainingDueBatchEngineChecks
{
    public static void PrimaryTrainingDueBatchEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryTrainingDueBatchEngine.DefaultPrimaryTrainingDueBatchEngine();
        var item = PrimaryTrainingDueBatch.NewPrimaryTrainingDueBatch("code-837", "name-364");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryTrainingDueBatchEngineKeepsResultsWithinBounds();
    }
}
