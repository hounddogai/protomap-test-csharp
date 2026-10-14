namespace Acme.Workforce.Compliance.Tests.TrainingDue.Draft;

using Acme.Workforce.Compliance.TrainingDue.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class BatchedTrainingDueDraftViewChecks
{
    public static void BatchedTrainingDueDraftViewShowsTheStatusInCapitals()
    {
        var item = BatchedTrainingDueDraft.NewBatchedTrainingDueDraft("code-459", "title-936");
        var view = BatchedTrainingDueDraftView.ToBatchedTrainingDueDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedTrainingDueDraftView.ToBatchedTrainingDueDraftViews(new List<BatchedTrainingDueDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedTrainingDueDraftViewShowsTheStatusInCapitals();
    }
}
