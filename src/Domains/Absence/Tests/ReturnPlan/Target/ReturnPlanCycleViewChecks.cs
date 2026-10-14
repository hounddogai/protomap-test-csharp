namespace Acme.Workforce.Absence.Tests.ReturnPlan.Target;

using Acme.Workforce.Absence.ReturnPlan.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class ReturnPlanCycleViewChecks
{
    public static void ReturnPlanCycleViewShowsTheStatusInCapitals()
    {
        var item = ReturnPlanCycle.NewReturnPlanCycle("code-170", "title-842");
        var view = ReturnPlanCycleView.ToReturnPlanCycleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ReturnPlanCycleView.ToReturnPlanCycleViews(new List<ReturnPlanCycle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ReturnPlanCycleViewShowsTheStatusInCapitals();
    }
}
