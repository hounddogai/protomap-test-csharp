namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeBudget;

using Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

/// <summary>Checks of badge budget code, run by the project's runner.</summary>
public static class RollingAccessBadgeProfileViewChecks
{
    public static void RollingAccessBadgeProfileViewShowsTheStatusInCapitals()
    {
        var item = RollingAccessBadgeProfile.NewRollingAccessBadgeProfile("code-54", "label-151");
        var view = RollingAccessBadgeProfileView.ToRollingAccessBadgeProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingAccessBadgeProfileView.ToRollingAccessBadgeProfileViews(new List<RollingAccessBadgeProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingAccessBadgeProfileViewShowsTheStatusInCapitals();
    }
}
