namespace Acme.Workforce.Compensation.Tests.MeritCycle.Budget;

using Acme.Workforce.Compensation.MeritCycle.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class BatchedMeritCycleReviewViewChecks
{
    public static void BatchedMeritCycleReviewViewShowsTheStatusInCapitals()
    {
        var item = BatchedMeritCycleReview.NewBatchedMeritCycleReview("code-492", "title-864");
        var view = BatchedMeritCycleReviewView.ToBatchedMeritCycleReviewView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedMeritCycleReviewView.ToBatchedMeritCycleReviewViews(new List<BatchedMeritCycleReview> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedMeritCycleReviewViewShowsTheStatusInCapitals();
    }
}
