namespace Acme.Workforce.Compliance.Tests.TrainingDue.Variance;

using Acme.Workforce.Compliance.TrainingDue.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class TrainingDueVarianceViewChecks
{
    public static void TrainingDueVarianceViewShowsTheStatusInCapitals()
    {
        var item = TrainingDueVariance.NewTrainingDueVariance("code-873", "name-160");
        var view = TrainingDueVarianceView.ToTrainingDueVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TrainingDueVarianceView.ToTrainingDueVarianceViews(new List<TrainingDueVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TrainingDueVarianceViewShowsTheStatusInCapitals();
    }
}
