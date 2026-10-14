namespace Acme.Workforce.Absence.Tests.ReturnPlan.Metric;

using Acme.Workforce.Absence.ReturnPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ReturnPlanEstimateViewChecks
{
    public static void ReturnPlanEstimateViewShowsTheStatusInCapitals()
    {
        var item = ReturnPlanEstimate.NewReturnPlanEstimate("code-672", "name-674");
        var view = ReturnPlanEstimateView.ToReturnPlanEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ReturnPlanEstimateView.ToReturnPlanEstimateViews(new List<ReturnPlanEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ReturnPlanEstimateViewShowsTheStatusInCapitals();
    }
}
