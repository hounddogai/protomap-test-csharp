namespace Acme.Workforce.Contractors.Tests.Engagement.Profile;

using Acme.Workforce.Contractors.Engagement.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class EngagementProfileViewChecks
{
    public static void EngagementProfileViewShowsTheStatusInCapitals()
    {
        var item = EngagementProfile.NewEngagementProfile("code-277", "name-353");
        var view = EngagementProfileView.ToEngagementProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EngagementProfileView.ToEngagementProfileViews(new List<EngagementProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EngagementProfileViewShowsTheStatusInCapitals();
    }
}
