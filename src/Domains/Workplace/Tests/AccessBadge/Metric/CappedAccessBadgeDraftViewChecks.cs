namespace Acme.Workforce.Workplace.Tests.AccessBadge.Metric;

using Acme.Workforce.Workplace.AccessBadge.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class CappedAccessBadgeDraftViewChecks
{
    public static void CappedAccessBadgeDraftViewShowsTheStatusInCapitals()
    {
        var item = CappedAccessBadgeDraft.NewCappedAccessBadgeDraft("code-412", "label-655");
        var view = CappedAccessBadgeDraftView.ToCappedAccessBadgeDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedAccessBadgeDraftView.ToCappedAccessBadgeDraftViews(new List<CappedAccessBadgeDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedAccessBadgeDraftViewShowsTheStatusInCapitals();
    }
}
