namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeEntry;

using Acme.Workforce.Workplace.AccessBadge.BadgeEntry;

/// <summary>Checks of badge entry code, run by the project's runner.</summary>
public static class LenientAccessBadgeEntryViewChecks
{
    public static void LenientAccessBadgeEntryViewShowsTheStatusInCapitals()
    {
        var item = LenientAccessBadgeEntry.NewLenientAccessBadgeEntry("code-94", "title-131");
        var view = LenientAccessBadgeEntryView.ToLenientAccessBadgeEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientAccessBadgeEntryView.ToLenientAccessBadgeEntryViews(new List<LenientAccessBadgeEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientAccessBadgeEntryViewShowsTheStatusInCapitals();
    }
}
