namespace Acme.Workforce.Compliance.Tests.Exception.SeasonalExceptionGroup;

using Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

/// <summary>Checks of seasonal exception group code, run by the project's runner.</summary>
public static class ExceptionMilestoneViewChecks
{
    public static void ExceptionMilestoneViewShowsTheStatusInCapitals()
    {
        var item = ExceptionMilestone.NewExceptionMilestone("code-607", "title-283");
        var view = ExceptionMilestoneView.ToExceptionMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ExceptionMilestoneView.ToExceptionMilestoneViews(new List<ExceptionMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ExceptionMilestoneViewShowsTheStatusInCapitals();
    }
}
