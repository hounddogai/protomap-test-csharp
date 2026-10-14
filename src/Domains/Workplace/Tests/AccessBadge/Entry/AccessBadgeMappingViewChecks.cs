namespace Acme.Workforce.Workplace.Tests.AccessBadge.Entry;

using Acme.Workforce.Workplace.AccessBadge.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class AccessBadgeMappingViewChecks
{
    public static void AccessBadgeMappingViewShowsTheStatusInCapitals()
    {
        var item = AccessBadgeMapping.NewAccessBadgeMapping("code-779", "name-618");
        var view = AccessBadgeMappingView.ToAccessBadgeMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AccessBadgeMappingView.ToAccessBadgeMappingViews(new List<AccessBadgeMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AccessBadgeMappingViewShowsTheStatusInCapitals();
    }
}
