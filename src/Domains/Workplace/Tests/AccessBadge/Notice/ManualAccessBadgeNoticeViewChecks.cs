namespace Acme.Workforce.Workplace.Tests.AccessBadge.Notice;

using Acme.Workforce.Workplace.AccessBadge.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class ManualAccessBadgeNoticeViewChecks
{
    public static void ManualAccessBadgeNoticeViewShowsTheStatusInCapitals()
    {
        var item = ManualAccessBadgeNotice.NewManualAccessBadgeNotice("code-998", "name-643");
        var view = ManualAccessBadgeNoticeView.ToManualAccessBadgeNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualAccessBadgeNoticeView.ToManualAccessBadgeNoticeViews(new List<ManualAccessBadgeNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualAccessBadgeNoticeViewShowsTheStatusInCapitals();
    }
}
