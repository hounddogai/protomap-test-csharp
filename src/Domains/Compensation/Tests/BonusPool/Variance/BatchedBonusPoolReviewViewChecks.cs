namespace Acme.Workforce.Compensation.Tests.BonusPool.Variance;

using Acme.Workforce.Compensation.BonusPool.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class BatchedBonusPoolReviewViewChecks
{
    public static void BatchedBonusPoolReviewViewShowsTheStatusInCapitals()
    {
        var item = BatchedBonusPoolReview.NewBatchedBonusPoolReview("code-112", "title-74");
        var view = BatchedBonusPoolReviewView.ToBatchedBonusPoolReviewView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedBonusPoolReviewView.ToBatchedBonusPoolReviewViews(new List<BatchedBonusPoolReview> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedBonusPoolReviewViewShowsTheStatusInCapitals();
    }
}
