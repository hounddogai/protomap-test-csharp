namespace Acme.Workforce.Compliance.Tests.TrainingDue.Metric;

using Acme.Workforce.Compliance.TrainingDue.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class FallbackTrainingDueRollupViewChecks
{
    public static void FallbackTrainingDueRollupViewShowsTheStatusInCapitals()
    {
        var item = FallbackTrainingDueRollup.NewFallbackTrainingDueRollup("code-850", "title-688");
        var view = FallbackTrainingDueRollupView.ToFallbackTrainingDueRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackTrainingDueRollupView.ToFallbackTrainingDueRollupViews(new List<FallbackTrainingDueRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackTrainingDueRollupViewShowsTheStatusInCapitals();
    }
}
