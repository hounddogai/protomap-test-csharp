namespace Acme.Workforce.Absence.Tests.Accrual.Slot;

using Acme.Workforce.Absence.Accrual.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class AccrualPolicyViewChecks
{
    public static void AccrualPolicyViewShowsTheStatusInCapitals()
    {
        var item = AccrualPolicy.NewAccrualPolicy("code-958", "label-944");
        var view = AccrualPolicyView.ToAccrualPolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AccrualPolicyView.ToAccrualPolicyViews(new List<AccrualPolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AccrualPolicyViewShowsTheStatusInCapitals();
    }
}
