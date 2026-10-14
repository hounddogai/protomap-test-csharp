namespace Acme.Workforce.Orgdesign.Tests.Reorg.Plan;

using Acme.Workforce.Orgdesign.Reorg.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class PendingReorgEntryViewChecks
{
    public static void PendingReorgEntryViewShowsTheStatusInCapitals()
    {
        var item = PendingReorgEntry.NewPendingReorgEntry("code-206", "title-841");
        var view = PendingReorgEntryView.ToPendingReorgEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingReorgEntryView.ToPendingReorgEntryViews(new List<PendingReorgEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingReorgEntryViewShowsTheStatusInCapitals();
    }
}
