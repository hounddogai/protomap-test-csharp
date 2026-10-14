namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueTier;

using Acme.Workforce.Compliance.TrainingDue.DueTier;

/// <summary>Checks of due tier code, run by the project's runner.</summary>
public static class BatchedTrainingDueNoticeChecks
{
    public static void BatchedTrainingDueNoticeStartsInIdle()
    {
        var item = BatchedTrainingDueNotice.NewBatchedTrainingDueNotice("code-785", "name-139");
        Check.Equal(item.Status, BatchedTrainingDueNoticeStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedTrainingDueNoticeMovesToWarming()
    {
        var item = BatchedTrainingDueNotice.NewBatchedTrainingDueNotice("code-785", "name-139");
        try
        {
            item.MoveTo(BatchedTrainingDueNoticeStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedTrainingDueNoticeStatus.Warming, "status");
    }

    public static void BatchedTrainingDueNoticeRejectsMovesFromStopped()
    {
        var item = BatchedTrainingDueNotice.NewBatchedTrainingDueNotice("code-785", "name-139");
        item.Status = BatchedTrainingDueNoticeStatus.Stopped;
        Check.Fails(() => item.MoveTo(BatchedTrainingDueNoticeStatus.Idle), "move");
    }

    public static void RunAll()
    {
        BatchedTrainingDueNoticeStartsInIdle();
        BatchedTrainingDueNoticeMovesToWarming();
        BatchedTrainingDueNoticeRejectsMovesFromStopped();
    }
}
