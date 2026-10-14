namespace Acme.Workforce.Compliance.Tests.TrainingDue.CappedDueVariance;

using Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;

/// <summary>Checks of capped due variance code, run by the project's runner.</summary>
public static class PendingTrainingDueBatchChecks
{
    public static void PendingTrainingDueBatchStartsInOpen()
    {
        var item = PendingTrainingDueBatch.NewPendingTrainingDueBatch("code-478", "label-600");
        Check.Equal(item.Status, PendingTrainingDueBatchStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingTrainingDueBatchMovesToInReview()
    {
        var item = PendingTrainingDueBatch.NewPendingTrainingDueBatch("code-478", "label-600");
        try
        {
            item.MoveTo(PendingTrainingDueBatchStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingTrainingDueBatchStatus.InReview, "status");
    }

    public static void PendingTrainingDueBatchRejectsMovesFromArchived()
    {
        var item = PendingTrainingDueBatch.NewPendingTrainingDueBatch("code-478", "label-600");
        item.Status = PendingTrainingDueBatchStatus.Archived;
        Check.Fails(() => item.MoveTo(PendingTrainingDueBatchStatus.Open), "move");
    }

    public static void RunAll()
    {
        PendingTrainingDueBatchStartsInOpen();
        PendingTrainingDueBatchMovesToInReview();
        PendingTrainingDueBatchRejectsMovesFromArchived();
    }
}
