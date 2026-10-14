namespace Acme.Workforce.Compliance.Tests.TrainingDue.Mapping;

using Acme.Workforce.Compliance.TrainingDue.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class TrainingDueBatchViewChecks
{
    public static void TrainingDueBatchViewShowsTheStatusInCapitals()
    {
        var item = TrainingDueBatch.NewTrainingDueBatch("code-483", "name-286");
        var view = TrainingDueBatchView.ToTrainingDueBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TrainingDueBatchView.ToTrainingDueBatchViews(new List<TrainingDueBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TrainingDueBatchViewShowsTheStatusInCapitals();
    }
}
