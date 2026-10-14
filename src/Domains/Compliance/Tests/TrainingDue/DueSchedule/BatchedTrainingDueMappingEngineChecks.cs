namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueSchedule;

using Acme.Workforce.Compliance.TrainingDue.DueSchedule;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of due schedule code, run by the project's runner.</summary>
public static class BatchedTrainingDueMappingEngineChecks
{
    public static void BatchedTrainingDueMappingEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedTrainingDueMappingEngine.DefaultBatchedTrainingDueMappingEngine(Flags.NewRegistry());
        var item = BatchedTrainingDueMapping.NewBatchedTrainingDueMapping("code-991", "title-481");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedTrainingDueMappingEngineKeepsResultsWithinBounds();
    }
}
