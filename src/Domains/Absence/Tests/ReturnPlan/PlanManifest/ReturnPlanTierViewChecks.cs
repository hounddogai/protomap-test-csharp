namespace Acme.Workforce.Absence.Tests.ReturnPlan.PlanManifest;

using Acme.Workforce.Absence.ReturnPlan.PlanManifest;

/// <summary>Checks of plan manifest code, run by the project's runner.</summary>
public static class ReturnPlanTierViewChecks
{
    public static void ReturnPlanTierViewShowsTheStatusInCapitals()
    {
        var item = ReturnPlanTier.NewReturnPlanTier("code-807", "label-348");
        var view = ReturnPlanTierView.ToReturnPlanTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ReturnPlanTierView.ToReturnPlanTierViews(new List<ReturnPlanTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ReturnPlanTierViewShowsTheStatusInCapitals();
    }
}
