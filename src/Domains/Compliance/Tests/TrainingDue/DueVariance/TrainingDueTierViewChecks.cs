namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueVariance;

using Acme.Workforce.Compliance.TrainingDue.DueVariance;

/// <summary>Checks of due variance code, run by the project's runner.</summary>
public static class TrainingDueTierViewChecks
{
    public static void TrainingDueTierViewShowsTheStatusInCapitals()
    {
        var item = TrainingDueTier.NewTrainingDueTier("code-697", "label-128");
        var view = TrainingDueTierView.ToTrainingDueTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TrainingDueTierView.ToTrainingDueTierViews(new List<TrainingDueTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TrainingDueTierViewShowsTheStatusInCapitals();
    }
}
