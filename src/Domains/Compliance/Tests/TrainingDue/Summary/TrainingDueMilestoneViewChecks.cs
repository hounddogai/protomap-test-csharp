namespace Acme.Workforce.Compliance.Tests.TrainingDue.Summary;

using Acme.Workforce.Compliance.TrainingDue.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class TrainingDueMilestoneViewChecks
{
    public static void TrainingDueMilestoneViewShowsTheStatusInCapitals()
    {
        var item = TrainingDueMilestone.NewTrainingDueMilestone("code-138", "label-206");
        var view = TrainingDueMilestoneView.ToTrainingDueMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TrainingDueMilestoneView.ToTrainingDueMilestoneViews(new List<TrainingDueMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TrainingDueMilestoneViewShowsTheStatusInCapitals();
    }
}
