namespace Acme.Workforce.Absence.Tests.Accrual.Schedule;

using Acme.Workforce.Absence.Accrual.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class ManualAccrualPolicyViewChecks
{
    public static void ManualAccrualPolicyViewShowsTheStatusInCapitals()
    {
        var item = ManualAccrualPolicy.NewManualAccrualPolicy("code-193", "name-557");
        var view = ManualAccrualPolicyView.ToManualAccrualPolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualAccrualPolicyView.ToManualAccrualPolicyViews(new List<ManualAccrualPolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualAccrualPolicyViewShowsTheStatusInCapitals();
    }
}
