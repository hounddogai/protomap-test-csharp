namespace Acme.Workforce.Compliance.Tests.TrainingDue.Plan;

using Acme.Workforce.Compliance.TrainingDue.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class TrainingDueProfileViewChecks
{
    public static void TrainingDueProfileViewShowsTheStatusInCapitals()
    {
        var item = TrainingDueProfile.NewTrainingDueProfile("code-900", "label-500");
        var view = TrainingDueProfileView.ToTrainingDueProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TrainingDueProfileView.ToTrainingDueProfileViews(new List<TrainingDueProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TrainingDueProfileViewShowsTheStatusInCapitals();
    }
}
