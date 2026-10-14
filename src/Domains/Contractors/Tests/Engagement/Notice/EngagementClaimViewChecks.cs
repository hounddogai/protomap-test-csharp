namespace Acme.Workforce.Contractors.Tests.Engagement.Notice;

using Acme.Workforce.Contractors.Engagement.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class EngagementClaimViewChecks
{
    public static void EngagementClaimViewShowsTheStatusInCapitals()
    {
        var item = EngagementClaim.NewEngagementClaim("code-139", "label-687");
        var view = EngagementClaimView.ToEngagementClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EngagementClaimView.ToEngagementClaimViews(new List<EngagementClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EngagementClaimViewShowsTheStatusInCapitals();
    }
}
