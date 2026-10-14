namespace Acme.Workforce.Compliance.Tests.TrainingDue.Metric;

using Acme.Workforce.Compliance.TrainingDue.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class SeasonalTrainingDueBaselineViewChecks
{
    public static void SeasonalTrainingDueBaselineViewShowsTheStatusInCapitals()
    {
        var item = SeasonalTrainingDueBaseline.NewSeasonalTrainingDueBaseline("code-28", "name-148");
        var view = SeasonalTrainingDueBaselineView.ToSeasonalTrainingDueBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalTrainingDueBaselineView.ToSeasonalTrainingDueBaselineViews(new List<SeasonalTrainingDueBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalTrainingDueBaselineViewShowsTheStatusInCapitals();
    }
}
