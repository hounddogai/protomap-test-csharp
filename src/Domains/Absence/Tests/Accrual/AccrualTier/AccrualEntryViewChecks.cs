namespace Acme.Workforce.Absence.Tests.Accrual.AccrualTier;

using Acme.Workforce.Absence.Accrual.AccrualTier;

/// <summary>Checks of accrual tier code, run by the project's runner.</summary>
public static class AccrualEntryViewChecks
{
    public static void AccrualEntryViewShowsTheStatusInCapitals()
    {
        var item = AccrualEntry.NewAccrualEntry("code-68", "label-464");
        var view = AccrualEntryView.ToAccrualEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AccrualEntryView.ToAccrualEntryViews(new List<AccrualEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AccrualEntryViewShowsTheStatusInCapitals();
    }
}
