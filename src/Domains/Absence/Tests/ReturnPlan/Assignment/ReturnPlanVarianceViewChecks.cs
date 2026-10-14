namespace Acme.Workforce.Absence.Tests.ReturnPlan.Assignment;

using Acme.Workforce.Absence.ReturnPlan.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class ReturnPlanVarianceViewChecks
{
    public static void ReturnPlanVarianceViewShowsTheStatusInCapitals()
    {
        var item = ReturnPlanVariance.NewReturnPlanVariance("code-977", "name-35");
        var view = ReturnPlanVarianceView.ToReturnPlanVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ReturnPlanVarianceView.ToReturnPlanVarianceViews(new List<ReturnPlanVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ReturnPlanVarianceViewShowsTheStatusInCapitals();
    }
}
