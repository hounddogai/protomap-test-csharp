namespace Acme.Workforce.Compliance.Tests.TrainingDue.Batch;

using Acme.Workforce.Compliance.TrainingDue.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class TrainingDueQuotaEngineChecks
{
    public static void TrainingDueQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = TrainingDueQuotaEngine.DefaultTrainingDueQuotaEngine();
        var item = TrainingDueQuota.NewTrainingDueQuota("code-867", "label-76");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TrainingDueQuotaEngineKeepsResultsWithinBounds();
    }
}
