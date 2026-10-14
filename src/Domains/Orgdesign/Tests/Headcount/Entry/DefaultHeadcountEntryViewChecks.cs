namespace Acme.Workforce.Orgdesign.Tests.Headcount.Entry;

using Acme.Workforce.Orgdesign.Headcount.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class DefaultHeadcountEntryViewChecks
{
    public static void DefaultHeadcountEntryViewShowsTheStatusInCapitals()
    {
        var item = DefaultHeadcountEntry.NewDefaultHeadcountEntry("code-98", "label-802");
        var view = DefaultHeadcountEntryView.ToDefaultHeadcountEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultHeadcountEntryView.ToDefaultHeadcountEntryViews(new List<DefaultHeadcountEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultHeadcountEntryViewShowsTheStatusInCapitals();
    }
}
