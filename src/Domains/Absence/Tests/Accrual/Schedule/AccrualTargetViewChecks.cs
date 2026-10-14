namespace Acme.Workforce.Absence.Tests.Accrual.Schedule;

using Acme.Workforce.Absence.Accrual.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class AccrualTargetViewChecks
{
    public static void AccrualTargetViewShowsTheStatusInCapitals()
    {
        var item = AccrualTarget.NewAccrualTarget("code-866", "name-940");
        var view = AccrualTargetView.ToAccrualTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AccrualTargetView.ToAccrualTargetViews(new List<AccrualTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AccrualTargetViewShowsTheStatusInCapitals();
    }
}
