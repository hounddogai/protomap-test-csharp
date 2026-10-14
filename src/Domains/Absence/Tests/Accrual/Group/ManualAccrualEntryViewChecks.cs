namespace Acme.Workforce.Absence.Tests.Accrual.Group;

using Acme.Workforce.Absence.Accrual.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ManualAccrualEntryViewChecks
{
    public static void ManualAccrualEntryViewShowsTheStatusInCapitals()
    {
        var item = ManualAccrualEntry.NewManualAccrualEntry("code-663", "label-971");
        var view = ManualAccrualEntryView.ToManualAccrualEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualAccrualEntryView.ToManualAccrualEntryViews(new List<ManualAccrualEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualAccrualEntryViewShowsTheStatusInCapitals();
    }
}
