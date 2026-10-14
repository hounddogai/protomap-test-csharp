namespace Acme.Workforce.Workplace.Tests.AccessBadge.Ledger;

using Acme.Workforce.Workplace.AccessBadge.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class DefaultAccessBadgeBundleViewChecks
{
    public static void DefaultAccessBadgeBundleViewShowsTheStatusInCapitals()
    {
        var item = DefaultAccessBadgeBundle.NewDefaultAccessBadgeBundle("code-94", "name-212");
        var view = DefaultAccessBadgeBundleView.ToDefaultAccessBadgeBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultAccessBadgeBundleView.ToDefaultAccessBadgeBundleViews(new List<DefaultAccessBadgeBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultAccessBadgeBundleViewShowsTheStatusInCapitals();
    }
}
