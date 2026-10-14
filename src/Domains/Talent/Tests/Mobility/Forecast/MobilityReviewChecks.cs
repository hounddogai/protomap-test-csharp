namespace Acme.Workforce.Talent.Tests.Mobility.Forecast;

using Acme.Workforce.Talent.Mobility.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class MobilityReviewChecks
{
    public static void MobilityReviewStartsInCreated()
    {
        var item = MobilityReview.NewMobilityReview("code-547", "label-723");
        Check.Equal(item.Status, MobilityReviewStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MobilityReviewMovesToPublished()
    {
        var item = MobilityReview.NewMobilityReview("code-547", "label-723");
        try
        {
            item.MoveTo(MobilityReviewStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MobilityReviewStatus.Published, "status");
    }

    public static void MobilityReviewRejectsMovesFromRetired()
    {
        var item = MobilityReview.NewMobilityReview("code-547", "label-723");
        item.Status = MobilityReviewStatus.Retired;
        Check.Fails(() => item.MoveTo(MobilityReviewStatus.Created), "move");
    }

    public static void RunAll()
    {
        MobilityReviewStartsInCreated();
        MobilityReviewMovesToPublished();
        MobilityReviewRejectsMovesFromRetired();
    }
}
