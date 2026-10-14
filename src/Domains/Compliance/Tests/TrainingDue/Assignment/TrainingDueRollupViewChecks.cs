namespace Acme.Workforce.Compliance.Tests.TrainingDue.Assignment;

using Acme.Workforce.Compliance.TrainingDue.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class TrainingDueRollupViewChecks
{
    public static void TrainingDueRollupViewShowsTheStatusInCapitals()
    {
        var item = TrainingDueRollup.NewTrainingDueRollup("code-985", "label-559");
        var view = TrainingDueRollupView.ToTrainingDueRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TrainingDueRollupView.ToTrainingDueRollupViews(new List<TrainingDueRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TrainingDueRollupViewShowsTheStatusInCapitals();
    }
}
