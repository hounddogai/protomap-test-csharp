namespace Acme.Workforce.Compliance.Tests.TrainingDue.StrictDueVariance;

using Acme.Workforce.Compliance.TrainingDue.StrictDueVariance;

/// <summary>Checks of strict due variance code, run by the project's runner.</summary>
public static class TrainingDueBaselineViewChecks
{
    public static void TrainingDueBaselineViewShowsTheStatusInCapitals()
    {
        var item = TrainingDueBaseline.NewTrainingDueBaseline("code-558", "label-587");
        var view = TrainingDueBaselineView.ToTrainingDueBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TrainingDueBaselineView.ToTrainingDueBaselineViews(new List<TrainingDueBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TrainingDueBaselineViewShowsTheStatusInCapitals();
    }
}
