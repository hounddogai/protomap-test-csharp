namespace Acme.Workforce.Compliance.Tests.TrainingDue.CappedDueVariance;

using Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;

/// <summary>Checks of capped due variance code, run by the project's runner.</summary>
public static class PendingTrainingDueBatchViewChecks
{
    public static void PendingTrainingDueBatchViewShowsTheStatusInCapitals()
    {
        var item = PendingTrainingDueBatch.NewPendingTrainingDueBatch("code-478", "label-600");
        var view = PendingTrainingDueBatchView.ToPendingTrainingDueBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingTrainingDueBatchView.ToPendingTrainingDueBatchViews(new List<PendingTrainingDueBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingTrainingDueBatchViewShowsTheStatusInCapitals();
    }
}
