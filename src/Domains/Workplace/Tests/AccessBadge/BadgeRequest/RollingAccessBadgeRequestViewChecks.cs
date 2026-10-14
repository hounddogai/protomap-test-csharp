namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeRequest;

using Acme.Workforce.Workplace.AccessBadge.BadgeRequest;

/// <summary>Checks of badge request code, run by the project's runner.</summary>
public static class RollingAccessBadgeRequestViewChecks
{
    public static void RollingAccessBadgeRequestViewShowsTheStatusInCapitals()
    {
        var item = RollingAccessBadgeRequest.NewRollingAccessBadgeRequest("code-645", "label-901");
        var view = RollingAccessBadgeRequestView.ToRollingAccessBadgeRequestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingAccessBadgeRequestView.ToRollingAccessBadgeRequestViews(new List<RollingAccessBadgeRequest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingAccessBadgeRequestViewShowsTheStatusInCapitals();
    }
}
