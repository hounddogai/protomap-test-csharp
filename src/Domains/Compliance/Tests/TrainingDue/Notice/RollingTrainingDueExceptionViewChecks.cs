namespace Acme.Workforce.Compliance.Tests.TrainingDue.Notice;

using Acme.Workforce.Compliance.TrainingDue.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class RollingTrainingDueExceptionViewChecks
{
    public static void RollingTrainingDueExceptionViewShowsTheStatusInCapitals()
    {
        var item = RollingTrainingDueException.NewRollingTrainingDueException("code-273", "name-751");
        var view = RollingTrainingDueExceptionView.ToRollingTrainingDueExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingTrainingDueExceptionView.ToRollingTrainingDueExceptionViews(new List<RollingTrainingDueException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingTrainingDueExceptionViewShowsTheStatusInCapitals();
    }
}
