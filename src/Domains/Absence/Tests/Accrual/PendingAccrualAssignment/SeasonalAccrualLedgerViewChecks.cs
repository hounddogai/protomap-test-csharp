namespace Acme.Workforce.Absence.Tests.Accrual.PendingAccrualAssignment;

using Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;

/// <summary>Checks of pending accrual assignment code, run by the project's runner.</summary>
public static class SeasonalAccrualLedgerViewChecks
{
    public static void SeasonalAccrualLedgerViewShowsTheStatusInCapitals()
    {
        var item = SeasonalAccrualLedger.NewSeasonalAccrualLedger("code-223", "title-998");
        var view = SeasonalAccrualLedgerView.ToSeasonalAccrualLedgerView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalAccrualLedgerView.ToSeasonalAccrualLedgerViews(new List<SeasonalAccrualLedger> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalAccrualLedgerViewShowsTheStatusInCapitals();
    }
}
