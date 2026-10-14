namespace Acme.Workforce.Absence.Tests.Accrual.Baseline;

using Acme.Workforce.Absence.Accrual.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PendingAccrualWindowViewChecks
{
    public static void PendingAccrualWindowViewShowsTheStatusInCapitals()
    {
        var item = PendingAccrualWindow.NewPendingAccrualWindow("code-930", "title-473");
        var view = PendingAccrualWindowView.ToPendingAccrualWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingAccrualWindowView.ToPendingAccrualWindowViews(new List<PendingAccrualWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingAccrualWindowViewShowsTheStatusInCapitals();
    }
}
