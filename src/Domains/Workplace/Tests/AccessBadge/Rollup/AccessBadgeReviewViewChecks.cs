namespace Acme.Workforce.Workplace.Tests.AccessBadge.Rollup;

using Acme.Workforce.Workplace.AccessBadge.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class AccessBadgeReviewViewChecks
{
    public static void AccessBadgeReviewViewShowsTheStatusInCapitals()
    {
        var item = AccessBadgeReview.NewAccessBadgeReview("code-286", "label-491");
        var view = AccessBadgeReviewView.ToAccessBadgeReviewView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AccessBadgeReviewView.ToAccessBadgeReviewViews(new List<AccessBadgeReview> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AccessBadgeReviewViewShowsTheStatusInCapitals();
    }
}
