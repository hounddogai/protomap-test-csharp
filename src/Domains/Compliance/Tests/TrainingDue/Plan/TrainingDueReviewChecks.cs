namespace Acme.Workforce.Compliance.Tests.TrainingDue.Plan;

using Acme.Workforce.Compliance.TrainingDue.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class TrainingDueReviewChecks
{
    public static void TrainingDueReviewStartsInNew()
    {
        var item = TrainingDueReview.NewTrainingDueReview("code-434", "label-533");
        Check.Equal(item.Status, TrainingDueReviewStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TrainingDueReviewMovesToQueued()
    {
        var item = TrainingDueReview.NewTrainingDueReview("code-434", "label-533");
        try
        {
            item.MoveTo(TrainingDueReviewStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TrainingDueReviewStatus.Queued, "status");
    }

    public static void TrainingDueReviewRejectsMovesFromFailed()
    {
        var item = TrainingDueReview.NewTrainingDueReview("code-434", "label-533");
        item.Status = TrainingDueReviewStatus.Failed;
        Check.Fails(() => item.MoveTo(TrainingDueReviewStatus.New), "move");
    }

    public static void RunAll()
    {
        TrainingDueReviewStartsInNew();
        TrainingDueReviewMovesToQueued();
        TrainingDueReviewRejectsMovesFromFailed();
    }
}
