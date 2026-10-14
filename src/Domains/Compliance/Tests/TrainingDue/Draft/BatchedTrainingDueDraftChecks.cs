namespace Acme.Workforce.Compliance.Tests.TrainingDue.Draft;

using Acme.Workforce.Compliance.TrainingDue.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class BatchedTrainingDueDraftChecks
{
    public static void BatchedTrainingDueDraftStartsInNew()
    {
        var item = BatchedTrainingDueDraft.NewBatchedTrainingDueDraft("code-459", "title-936");
        Check.Equal(item.Status, BatchedTrainingDueDraftStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedTrainingDueDraftMovesToDone()
    {
        var item = BatchedTrainingDueDraft.NewBatchedTrainingDueDraft("code-459", "title-936");
        try
        {
            item.MoveTo(BatchedTrainingDueDraftStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedTrainingDueDraftStatus.Done, "status");
    }

    public static void BatchedTrainingDueDraftRejectsMovesFromFailed()
    {
        var item = BatchedTrainingDueDraft.NewBatchedTrainingDueDraft("code-459", "title-936");
        item.Status = BatchedTrainingDueDraftStatus.Failed;
        Check.Fails(() => item.MoveTo(BatchedTrainingDueDraftStatus.New), "move");
    }

    public static void RunAll()
    {
        BatchedTrainingDueDraftStartsInNew();
        BatchedTrainingDueDraftMovesToDone();
        BatchedTrainingDueDraftRejectsMovesFromFailed();
    }
}
