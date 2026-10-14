namespace Acme.Workforce.Compliance.Tests.TrainingDue.OverrideItem;

using Acme.Workforce.Compliance.TrainingDue.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class TrainingDueCheckpointViewChecks
{
    public static void TrainingDueCheckpointViewShowsTheStatusInCapitals()
    {
        var item = TrainingDueCheckpoint.NewTrainingDueCheckpoint("code-863", "title-489");
        var view = TrainingDueCheckpointView.ToTrainingDueCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TrainingDueCheckpointView.ToTrainingDueCheckpointViews(new List<TrainingDueCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TrainingDueCheckpointViewShowsTheStatusInCapitals();
    }
}
