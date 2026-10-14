namespace Acme.Workforce.Workplace.Tests.AccessBadge.Policy;

using Acme.Workforce.Workplace.AccessBadge.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class AccessBadgeManifestViewChecks
{
    public static void AccessBadgeManifestViewShowsTheStatusInCapitals()
    {
        var item = AccessBadgeManifest.NewAccessBadgeManifest("code-997", "label-157");
        var view = AccessBadgeManifestView.ToAccessBadgeManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AccessBadgeManifestView.ToAccessBadgeManifestViews(new List<AccessBadgeManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AccessBadgeManifestViewShowsTheStatusInCapitals();
    }
}
