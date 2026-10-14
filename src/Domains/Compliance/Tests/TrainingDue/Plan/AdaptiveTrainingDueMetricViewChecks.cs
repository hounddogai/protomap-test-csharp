namespace Acme.Workforce.Compliance.Tests.TrainingDue.Plan;

using Acme.Workforce.Compliance.TrainingDue.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class AdaptiveTrainingDueMetricViewChecks
{
    public static void AdaptiveTrainingDueMetricViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveTrainingDueMetric.NewAdaptiveTrainingDueMetric("code-322", "label-365");
        var view = AdaptiveTrainingDueMetricView.ToAdaptiveTrainingDueMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveTrainingDueMetricView.ToAdaptiveTrainingDueMetricViews(new List<AdaptiveTrainingDueMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveTrainingDueMetricViewShowsTheStatusInCapitals();
    }
}
