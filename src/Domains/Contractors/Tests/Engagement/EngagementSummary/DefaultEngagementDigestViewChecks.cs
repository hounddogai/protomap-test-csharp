namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementSummary;

using Acme.Workforce.Contractors.Engagement.EngagementSummary;

/// <summary>Checks of engagement summary code, run by the project's runner.</summary>
public static class DefaultEngagementDigestViewChecks
{
    public static void DefaultEngagementDigestViewShowsTheStatusInCapitals()
    {
        var item = DefaultEngagementDigest.NewDefaultEngagementDigest("code-22", "label-202");
        var view = DefaultEngagementDigestView.ToDefaultEngagementDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultEngagementDigestView.ToDefaultEngagementDigestViews(new List<DefaultEngagementDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultEngagementDigestViewShowsTheStatusInCapitals();
    }
}
