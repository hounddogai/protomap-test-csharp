namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueSchedule;

using Acme.Workforce.Compliance.TrainingDue.DueSchedule;

/// <summary>Checks of due schedule code, run by the project's runner.</summary>
public static class BatchedTrainingDueMappingChecks
{
    public static void BatchedTrainingDueMappingStartsInIdle()
    {
        var item = BatchedTrainingDueMapping.NewBatchedTrainingDueMapping("code-991", "title-481");
        Check.Equal(item.Status, BatchedTrainingDueMappingStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedTrainingDueMappingMovesToDraining()
    {
        var item = BatchedTrainingDueMapping.NewBatchedTrainingDueMapping("code-991", "title-481");
        try
        {
            item.MoveTo(BatchedTrainingDueMappingStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedTrainingDueMappingStatus.Draining, "status");
    }

    public static void BatchedTrainingDueMappingRejectsMovesFromStopped()
    {
        var item = BatchedTrainingDueMapping.NewBatchedTrainingDueMapping("code-991", "title-481");
        item.Status = BatchedTrainingDueMappingStatus.Stopped;
        Check.Fails(() => item.MoveTo(BatchedTrainingDueMappingStatus.Idle), "move");
    }

    public static void RunAll()
    {
        BatchedTrainingDueMappingStartsInIdle();
        BatchedTrainingDueMappingMovesToDraining();
        BatchedTrainingDueMappingRejectsMovesFromStopped();
    }
}
