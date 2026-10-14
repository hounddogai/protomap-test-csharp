namespace Acme.Workforce.Compliance.Tests.TrainingDue.Summary;

using Acme.Workforce.Compliance.TrainingDue.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class TrainingDueTemplateViewChecks
{
    public static void TrainingDueTemplateViewShowsTheStatusInCapitals()
    {
        var item = TrainingDueTemplate.NewTrainingDueTemplate("code-901", "label-930");
        var view = TrainingDueTemplateView.ToTrainingDueTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TrainingDueTemplateView.ToTrainingDueTemplateViews(new List<TrainingDueTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TrainingDueTemplateViewShowsTheStatusInCapitals();
    }
}
