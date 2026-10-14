namespace Acme.Workforce.Absence.Tests.Accrual.Assignment;

using Acme.Workforce.Absence.Accrual.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class AccrualAllocationViewChecks
{
    public static void AccrualAllocationViewShowsTheStatusInCapitals()
    {
        var item = AccrualAllocation.NewAccrualAllocation("code-479", "name-399");
        var view = AccrualAllocationView.ToAccrualAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AccrualAllocationView.ToAccrualAllocationViews(new List<AccrualAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AccrualAllocationViewShowsTheStatusInCapitals();
    }
}
