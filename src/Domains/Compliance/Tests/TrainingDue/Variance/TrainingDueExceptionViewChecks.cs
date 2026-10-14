namespace Acme.Workforce.Compliance.Tests.TrainingDue.Variance;

using Acme.Workforce.Compliance.TrainingDue.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class TrainingDueExceptionViewChecks
{
    public static void TrainingDueExceptionViewShowsTheStatusInCapitals()
    {
        var item = TrainingDueException.NewTrainingDueException("code-327", "title-464");
        var view = TrainingDueExceptionView.ToTrainingDueExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TrainingDueExceptionView.ToTrainingDueExceptionViews(new List<TrainingDueException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TrainingDueExceptionViewShowsTheStatusInCapitals();
    }
}
