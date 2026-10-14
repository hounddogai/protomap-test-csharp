namespace Acme.Workforce.Payrollops.Tests.Garnishment.Forecast;

using Acme.Workforce.Payrollops.Garnishment.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class WeightedGarnishmentReviewChecks
{
    public static void WeightedGarnishmentReviewStartsInCreated()
    {
        var item = WeightedGarnishmentReview.NewWeightedGarnishmentReview("code-894", "title-599");
        Check.Equal(item.Status, WeightedGarnishmentReviewStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedGarnishmentReviewMovesToValidated()
    {
        var item = WeightedGarnishmentReview.NewWeightedGarnishmentReview("code-894", "title-599");
        try
        {
            item.MoveTo(WeightedGarnishmentReviewStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedGarnishmentReviewStatus.Validated, "status");
    }

    public static void WeightedGarnishmentReviewRejectsMovesFromRetired()
    {
        var item = WeightedGarnishmentReview.NewWeightedGarnishmentReview("code-894", "title-599");
        item.Status = WeightedGarnishmentReviewStatus.Retired;
        Check.Fails(() => item.MoveTo(WeightedGarnishmentReviewStatus.Created), "move");
    }

    public static void RunAll()
    {
        WeightedGarnishmentReviewStartsInCreated();
        WeightedGarnishmentReviewMovesToValidated();
        WeightedGarnishmentReviewRejectsMovesFromRetired();
    }
}
