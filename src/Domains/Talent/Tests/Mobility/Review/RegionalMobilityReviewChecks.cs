namespace Acme.Workforce.Talent.Tests.Mobility.Review;

using Acme.Workforce.Talent.Mobility.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RegionalMobilityReviewChecks
{
    public static void RegionalMobilityReviewStartsInNew()
    {
        var item = RegionalMobilityReview.NewRegionalMobilityReview("code-726", "title-198");
        Check.Equal(item.Status, RegionalMobilityReviewStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalMobilityReviewMovesToQueued()
    {
        var item = RegionalMobilityReview.NewRegionalMobilityReview("code-726", "title-198");
        try
        {
            item.MoveTo(RegionalMobilityReviewStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalMobilityReviewStatus.Queued, "status");
    }

    public static void RegionalMobilityReviewRejectsMovesFromFailed()
    {
        var item = RegionalMobilityReview.NewRegionalMobilityReview("code-726", "title-198");
        item.Status = RegionalMobilityReviewStatus.Failed;
        Check.Fails(() => item.MoveTo(RegionalMobilityReviewStatus.New), "move");
    }

    public static void RunAll()
    {
        RegionalMobilityReviewStartsInNew();
        RegionalMobilityReviewMovesToQueued();
        RegionalMobilityReviewRejectsMovesFromFailed();
    }
}
