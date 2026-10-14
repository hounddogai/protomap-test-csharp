namespace Acme.Workforce.Absence.Tests.Accrual.Threshold;

using Acme.Workforce.Absence.Accrual.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class AccrualWindowViewChecks
{
    public static void AccrualWindowViewShowsTheStatusInCapitals()
    {
        var item = AccrualWindow.NewAccrualWindow("code-890", "title-660");
        var view = AccrualWindowView.ToAccrualWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AccrualWindowView.ToAccrualWindowViews(new List<AccrualWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AccrualWindowViewShowsTheStatusInCapitals();
    }
}
