namespace Acme.Workforce.Compliance.Tests.TrainingDue.Batch;

using Acme.Workforce.Compliance.TrainingDue.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PrimaryTrainingDueBatchViewChecks
{
    public static void PrimaryTrainingDueBatchViewShowsTheStatusInCapitals()
    {
        var item = PrimaryTrainingDueBatch.NewPrimaryTrainingDueBatch("code-837", "name-364");
        var view = PrimaryTrainingDueBatchView.ToPrimaryTrainingDueBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryTrainingDueBatchView.ToPrimaryTrainingDueBatchViews(new List<PrimaryTrainingDueBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryTrainingDueBatchViewShowsTheStatusInCapitals();
    }
}
