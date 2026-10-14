namespace Acme.Workforce.Workplace.Tests.Desk.Entry;

using Acme.Workforce.Workplace.Desk.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class PendingDeskEntryViewChecks
{
    public static void PendingDeskEntryViewShowsTheStatusInCapitals()
    {
        var item = PendingDeskEntry.NewPendingDeskEntry("code-230", "label-39");
        var view = PendingDeskEntryView.ToPendingDeskEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingDeskEntryView.ToPendingDeskEntryViews(new List<PendingDeskEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingDeskEntryViewShowsTheStatusInCapitals();
    }
}
