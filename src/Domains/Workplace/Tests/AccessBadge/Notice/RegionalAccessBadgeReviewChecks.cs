namespace Acme.Workforce.Workplace.Tests.AccessBadge.Notice;

using Acme.Workforce.Workplace.AccessBadge.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class RegionalAccessBadgeReviewChecks
{
    public static void RegionalAccessBadgeReviewStartsInNew()
    {
        var item = RegionalAccessBadgeReview.NewRegionalAccessBadgeReview("code-734", "name-419");
        Check.Equal(item.Status, RegionalAccessBadgeReviewStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalAccessBadgeReviewMovesToQueued()
    {
        var item = RegionalAccessBadgeReview.NewRegionalAccessBadgeReview("code-734", "name-419");
        try
        {
            item.MoveTo(RegionalAccessBadgeReviewStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalAccessBadgeReviewStatus.Queued, "status");
    }

    public static void RegionalAccessBadgeReviewRejectsMovesFromFailed()
    {
        var item = RegionalAccessBadgeReview.NewRegionalAccessBadgeReview("code-734", "name-419");
        item.Status = RegionalAccessBadgeReviewStatus.Failed;
        Check.Fails(() => item.MoveTo(RegionalAccessBadgeReviewStatus.New), "move");
    }

    public static void RunAll()
    {
        RegionalAccessBadgeReviewStartsInNew();
        RegionalAccessBadgeReviewMovesToQueued();
        RegionalAccessBadgeReviewRejectsMovesFromFailed();
    }
}
