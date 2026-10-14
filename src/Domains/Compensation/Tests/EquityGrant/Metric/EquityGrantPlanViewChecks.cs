namespace Acme.Workforce.Compensation.Tests.EquityGrant.Metric;

using Acme.Workforce.Compensation.EquityGrant.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class EquityGrantPlanViewChecks
{
    public static void EquityGrantPlanViewShowsTheStatusInCapitals()
    {
        var item = EquityGrantPlan.NewEquityGrantPlan("code-128", "title-633");
        var view = EquityGrantPlanView.ToEquityGrantPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EquityGrantPlanView.ToEquityGrantPlanViews(new List<EquityGrantPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EquityGrantPlanViewShowsTheStatusInCapitals();
    }
}
