namespace Acme.Workforce.Absence.Tests.Accrual.Manifest;

using Acme.Workforce.Absence.Accrual.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class AccrualPlanViewChecks
{
    public static void AccrualPlanViewShowsTheStatusInCapitals()
    {
        var item = AccrualPlan.NewAccrualPlan("code-138", "title-827");
        var view = AccrualPlanView.ToAccrualPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AccrualPlanView.ToAccrualPlanViews(new List<AccrualPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AccrualPlanViewShowsTheStatusInCapitals();
    }
}
