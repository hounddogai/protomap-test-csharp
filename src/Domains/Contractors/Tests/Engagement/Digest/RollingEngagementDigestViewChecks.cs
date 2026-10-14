namespace Acme.Workforce.Contractors.Tests.Engagement.Digest;

using Acme.Workforce.Contractors.Engagement.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class RollingEngagementDigestViewChecks
{
    public static void RollingEngagementDigestViewShowsTheStatusInCapitals()
    {
        var item = RollingEngagementDigest.NewRollingEngagementDigest("code-581", "label-491");
        var view = RollingEngagementDigestView.ToRollingEngagementDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingEngagementDigestView.ToRollingEngagementDigestViews(new List<RollingEngagementDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingEngagementDigestViewShowsTheStatusInCapitals();
    }
}
