namespace Acme.Workforce.Absence.Tests.ReturnPlan.Ledger;

using Acme.Workforce.Absence.ReturnPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class ReturnPlanAllocationViewChecks
{
    public static void ReturnPlanAllocationViewShowsTheStatusInCapitals()
    {
        var item = ReturnPlanAllocation.NewReturnPlanAllocation("code-625", "name-28");
        var view = ReturnPlanAllocationView.ToReturnPlanAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ReturnPlanAllocationView.ToReturnPlanAllocationViews(new List<ReturnPlanAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ReturnPlanAllocationViewShowsTheStatusInCapitals();
    }
}
