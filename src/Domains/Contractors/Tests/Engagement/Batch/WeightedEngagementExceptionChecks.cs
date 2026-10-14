namespace Acme.Workforce.Contractors.Tests.Engagement.Batch;

using Acme.Workforce.Contractors.Engagement.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class WeightedEngagementExceptionChecks
{
    public static void WeightedEngagementExceptionStartsInOpen()
    {
        var item = WeightedEngagementException.NewWeightedEngagementException("code-101", "name-617");
        Check.Equal(item.Status, WeightedEngagementExceptionStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedEngagementExceptionMovesToInReview()
    {
        var item = WeightedEngagementException.NewWeightedEngagementException("code-101", "name-617");
        try
        {
            item.MoveTo(WeightedEngagementExceptionStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedEngagementExceptionStatus.InReview, "status");
    }

    public static void WeightedEngagementExceptionRejectsMovesFromArchived()
    {
        var item = WeightedEngagementException.NewWeightedEngagementException("code-101", "name-617");
        item.Status = WeightedEngagementExceptionStatus.Archived;
        Check.Fails(() => item.MoveTo(WeightedEngagementExceptionStatus.Open), "move");
    }

    public static void RunAll()
    {
        WeightedEngagementExceptionStartsInOpen();
        WeightedEngagementExceptionMovesToInReview();
        WeightedEngagementExceptionRejectsMovesFromArchived();
    }
}
