namespace Acme.Workforce.Workplace.Tests.AccessBadge.Tier;

using Acme.Workforce.Workplace.AccessBadge.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class DefaultAccessBadgeManifestViewChecks
{
    public static void DefaultAccessBadgeManifestViewShowsTheStatusInCapitals()
    {
        var item = DefaultAccessBadgeManifest.NewDefaultAccessBadgeManifest("code-720", "label-694");
        var view = DefaultAccessBadgeManifestView.ToDefaultAccessBadgeManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultAccessBadgeManifestView.ToDefaultAccessBadgeManifestViews(new List<DefaultAccessBadgeManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultAccessBadgeManifestViewShowsTheStatusInCapitals();
    }
}
