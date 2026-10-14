namespace Acme.Workforce.Orgdesign.Tests.Span.Entry;

using Acme.Workforce.Orgdesign.Span.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class GlobalSpanEntryViewChecks
{
    public static void GlobalSpanEntryViewShowsTheStatusInCapitals()
    {
        var item = GlobalSpanEntry.NewGlobalSpanEntry("code-524", "name-922");
        var view = GlobalSpanEntryView.ToGlobalSpanEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalSpanEntryView.ToGlobalSpanEntryViews(new List<GlobalSpanEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalSpanEntryViewShowsTheStatusInCapitals();
    }
}
