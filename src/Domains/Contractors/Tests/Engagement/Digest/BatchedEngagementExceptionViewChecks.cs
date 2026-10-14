namespace Acme.Workforce.Contractors.Tests.Engagement.Digest;

using Acme.Workforce.Contractors.Engagement.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class BatchedEngagementExceptionViewChecks
{
    public static void BatchedEngagementExceptionViewShowsTheStatusInCapitals()
    {
        var item = BatchedEngagementException.NewBatchedEngagementException("code-208", "label-893");
        var view = BatchedEngagementExceptionView.ToBatchedEngagementExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedEngagementExceptionView.ToBatchedEngagementExceptionViews(new List<BatchedEngagementException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedEngagementExceptionViewShowsTheStatusInCapitals();
    }
}
