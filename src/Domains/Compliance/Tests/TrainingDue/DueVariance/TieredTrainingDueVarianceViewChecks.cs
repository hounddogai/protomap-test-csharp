namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueVariance;

using Acme.Workforce.Compliance.TrainingDue.DueVariance;

/// <summary>Checks of due variance code, run by the project's runner.</summary>
public static class TieredTrainingDueVarianceViewChecks
{
    public static void TieredTrainingDueVarianceViewShowsTheStatusInCapitals()
    {
        var item = TieredTrainingDueVariance.NewTieredTrainingDueVariance("code-62", "title-708");
        var view = TieredTrainingDueVarianceView.ToTieredTrainingDueVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredTrainingDueVarianceView.ToTieredTrainingDueVarianceViews(new List<TieredTrainingDueVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredTrainingDueVarianceViewShowsTheStatusInCapitals();
    }
}
