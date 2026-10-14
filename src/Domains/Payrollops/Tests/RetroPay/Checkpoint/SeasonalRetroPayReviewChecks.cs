namespace Acme.Workforce.Payrollops.Tests.RetroPay.Checkpoint;

using Acme.Workforce.Payrollops.RetroPay.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class SeasonalRetroPayReviewChecks
{
    public static void SeasonalRetroPayReviewStartsInCreated()
    {
        var item = SeasonalRetroPayReview.NewSeasonalRetroPayReview("code-908", "name-603");
        Check.Equal(item.Status, SeasonalRetroPayReviewStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalRetroPayReviewMovesToValidated()
    {
        var item = SeasonalRetroPayReview.NewSeasonalRetroPayReview("code-908", "name-603");
        try
        {
            item.MoveTo(SeasonalRetroPayReviewStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalRetroPayReviewStatus.Validated, "status");
    }

    public static void SeasonalRetroPayReviewRejectsMovesFromRetired()
    {
        var item = SeasonalRetroPayReview.NewSeasonalRetroPayReview("code-908", "name-603");
        item.Status = SeasonalRetroPayReviewStatus.Retired;
        Check.Fails(() => item.MoveTo(SeasonalRetroPayReviewStatus.Created), "move");
    }

    public static void RunAll()
    {
        SeasonalRetroPayReviewStartsInCreated();
        SeasonalRetroPayReviewMovesToValidated();
        SeasonalRetroPayReviewRejectsMovesFromRetired();
    }
}
