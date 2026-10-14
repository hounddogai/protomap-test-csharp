namespace Acme.Workforce.Absence.Tests.Accrual.OverrideItem;

using Acme.Workforce.Absence.Accrual.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class AccrualOverrideViewChecks
{
    public static void AccrualOverrideViewShowsTheStatusInCapitals()
    {
        var item = AccrualOverride.NewAccrualOverride("code-645", "title-339");
        var view = AccrualOverrideView.ToAccrualOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AccrualOverrideView.ToAccrualOverrideViews(new List<AccrualOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AccrualOverrideViewShowsTheStatusInCapitals();
    }
}
