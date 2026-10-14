namespace Acme.Workforce.Contractors.Tests.Engagement.Manifest;

using Acme.Workforce.Contractors.Engagement.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class EngagementPolicyViewChecks
{
    public static void EngagementPolicyViewShowsTheStatusInCapitals()
    {
        var item = EngagementPolicy.NewEngagementPolicy("code-369", "title-91");
        var view = EngagementPolicyView.ToEngagementPolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EngagementPolicyView.ToEngagementPolicyViews(new List<EngagementPolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EngagementPolicyViewShowsTheStatusInCapitals();
    }
}
