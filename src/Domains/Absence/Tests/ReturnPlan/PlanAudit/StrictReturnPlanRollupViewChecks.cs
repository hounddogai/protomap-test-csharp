namespace Acme.Workforce.Absence.Tests.ReturnPlan.PlanAudit;

using Acme.Workforce.Absence.ReturnPlan.PlanAudit;

/// <summary>Checks of plan audit code, run by the project's runner.</summary>
public static class StrictReturnPlanRollupViewChecks
{
    public static void StrictReturnPlanRollupViewShowsTheStatusInCapitals()
    {
        var item = StrictReturnPlanRollup.NewStrictReturnPlanRollup("code-87", "title-623");
        var view = StrictReturnPlanRollupView.ToStrictReturnPlanRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictReturnPlanRollupView.ToStrictReturnPlanRollupViews(new List<StrictReturnPlanRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictReturnPlanRollupViewShowsTheStatusInCapitals();
    }
}
