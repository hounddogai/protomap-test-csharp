namespace Acme.Workforce.Workplace.Tests.AccessBadge.Rollup;

using Acme.Workforce.Workplace.AccessBadge.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class AccessBadgeReviewChecks
{
    public static void AccessBadgeReviewStartsInOpen()
    {
        var item = AccessBadgeReview.NewAccessBadgeReview("code-286", "label-491");
        Check.Equal(item.Status, AccessBadgeReviewStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccessBadgeReviewMovesToArchived()
    {
        var item = AccessBadgeReview.NewAccessBadgeReview("code-286", "label-491");
        try
        {
            item.MoveTo(AccessBadgeReviewStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccessBadgeReviewStatus.Archived, "status");
    }

    public static void AccessBadgeReviewRejectsMovesFromArchived()
    {
        var item = AccessBadgeReview.NewAccessBadgeReview("code-286", "label-491");
        item.Status = AccessBadgeReviewStatus.Archived;
        Check.Fails(() => item.MoveTo(AccessBadgeReviewStatus.Open), "move");
    }

    public static void RunAll()
    {
        AccessBadgeReviewStartsInOpen();
        AccessBadgeReviewMovesToArchived();
        AccessBadgeReviewRejectsMovesFromArchived();
    }
}
