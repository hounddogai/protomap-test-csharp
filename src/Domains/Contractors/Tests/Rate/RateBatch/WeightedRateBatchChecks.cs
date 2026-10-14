namespace Acme.Workforce.Contractors.Tests.Rate.RateBatch;

using Acme.Workforce.Contractors.Rate.RateBatch;

/// <summary>Checks of rate batch code, run by the project's runner.</summary>
public static class WeightedRateBatchChecks
{
    public static void WeightedRateBatchStartsInOpen()
    {
        var item = WeightedRateBatch.NewWeightedRateBatch("code-229", "title-480");
        Check.Equal(item.Status, WeightedRateBatchStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedRateBatchMovesToInReview()
    {
        var item = WeightedRateBatch.NewWeightedRateBatch("code-229", "title-480");
        try
        {
            item.MoveTo(WeightedRateBatchStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedRateBatchStatus.InReview, "status");
    }

    public static void WeightedRateBatchRejectsMovesFromArchived()
    {
        var item = WeightedRateBatch.NewWeightedRateBatch("code-229", "title-480");
        item.Status = WeightedRateBatchStatus.Archived;
        Check.Fails(() => item.MoveTo(WeightedRateBatchStatus.Open), "move");
    }

    public static void RunAll()
    {
        WeightedRateBatchStartsInOpen();
        WeightedRateBatchMovesToInReview();
        WeightedRateBatchRejectsMovesFromArchived();
    }
}
