namespace Acme.Workforce.Orgdesign.Tests.Reorg.Exception;

using Acme.Workforce.Orgdesign.Reorg.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class ReorgEntryViewChecks
{
    public static void ReorgEntryViewShowsTheStatusInCapitals()
    {
        var item = ReorgEntry.NewReorgEntry("code-81", "name-175");
        var view = ReorgEntryView.ToReorgEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ReorgEntryView.ToReorgEntryViews(new List<ReorgEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ReorgEntryViewShowsTheStatusInCapitals();
    }
}
