namespace Acme.Workforce.Contractors.Tests.Engagement.Score;

using Acme.Workforce.Contractors.Engagement.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class EngagementDigestViewChecks
{
    public static void EngagementDigestViewShowsTheStatusInCapitals()
    {
        var item = EngagementDigest.NewEngagementDigest("code-639", "name-312");
        var view = EngagementDigestView.ToEngagementDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EngagementDigestView.ToEngagementDigestViews(new List<EngagementDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EngagementDigestViewShowsTheStatusInCapitals();
    }
}
