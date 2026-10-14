namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueTier;

using Acme.Workforce.Compliance.TrainingDue.DueTier;

/// <summary>Checks of due tier code, run by the project's runner.</summary>
public static class BatchedTrainingDueNoticeViewChecks
{
    public static void BatchedTrainingDueNoticeViewShowsTheStatusInCapitals()
    {
        var item = BatchedTrainingDueNotice.NewBatchedTrainingDueNotice("code-785", "name-139");
        var view = BatchedTrainingDueNoticeView.ToBatchedTrainingDueNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedTrainingDueNoticeView.ToBatchedTrainingDueNoticeViews(new List<BatchedTrainingDueNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedTrainingDueNoticeViewShowsTheStatusInCapitals();
    }
}
