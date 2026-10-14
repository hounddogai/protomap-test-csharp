namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueWindow;

using Acme.Workforce.Compliance.TrainingDue.DueWindow;

/// <summary>Checks of due window code, run by the project's runner.</summary>
public static class AdaptiveTrainingDueExceptionViewChecks
{
    public static void AdaptiveTrainingDueExceptionViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveTrainingDueException.NewAdaptiveTrainingDueException("code-245", "name-143");
        var view = AdaptiveTrainingDueExceptionView.ToAdaptiveTrainingDueExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveTrainingDueExceptionView.ToAdaptiveTrainingDueExceptionViews(new List<AdaptiveTrainingDueException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveTrainingDueExceptionViewShowsTheStatusInCapitals();
    }
}
