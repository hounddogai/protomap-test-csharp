namespace Acme.Workforce.Workplace.Tests.Visitor.Entry;

using Acme.Workforce.Workplace.Visitor.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class VisitorEntryViewChecks
{
    public static void VisitorEntryViewShowsTheStatusInCapitals()
    {
        var item = VisitorEntry.NewVisitorEntry("code-70", "title-947");
        var view = VisitorEntryView.ToVisitorEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(VisitorEntryView.ToVisitorEntryViews(new List<VisitorEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        VisitorEntryViewShowsTheStatusInCapitals();
    }
}
