namespace Acme.Workforce.Absence.Tests.Accrual.OverrideItem;

using Acme.Workforce.Absence.Accrual.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RollingAccrualWindowViewChecks
{
    public static void RollingAccrualWindowViewShowsTheStatusInCapitals()
    {
        var item = RollingAccrualWindow.NewRollingAccrualWindow("code-645", "title-187");
        var view = RollingAccrualWindowView.ToRollingAccrualWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingAccrualWindowView.ToRollingAccrualWindowViews(new List<RollingAccrualWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingAccrualWindowViewShowsTheStatusInCapitals();
    }
}
