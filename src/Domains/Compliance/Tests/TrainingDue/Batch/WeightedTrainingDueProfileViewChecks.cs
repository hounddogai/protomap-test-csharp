namespace Acme.Workforce.Compliance.Tests.TrainingDue.Batch;

using Acme.Workforce.Compliance.TrainingDue.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class WeightedTrainingDueProfileViewChecks
{
    public static void WeightedTrainingDueProfileViewShowsTheStatusInCapitals()
    {
        var item = WeightedTrainingDueProfile.NewWeightedTrainingDueProfile("code-83", "title-448");
        var view = WeightedTrainingDueProfileView.ToWeightedTrainingDueProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedTrainingDueProfileView.ToWeightedTrainingDueProfileViews(new List<WeightedTrainingDueProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedTrainingDueProfileViewShowsTheStatusInCapitals();
    }
}
