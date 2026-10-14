namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Milestone;

using Acme.Workforce.Talent.SuccessionPlan.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class SuccessionPlanExceptionViewChecks
{
    public static void SuccessionPlanExceptionViewShowsTheStatusInCapitals()
    {
        var item = SuccessionPlanException.NewSuccessionPlanException("code-152", "title-196");
        var view = SuccessionPlanExceptionView.ToSuccessionPlanExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SuccessionPlanExceptionView.ToSuccessionPlanExceptionViews(new List<SuccessionPlanException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SuccessionPlanExceptionViewShowsTheStatusInCapitals();
    }
}
