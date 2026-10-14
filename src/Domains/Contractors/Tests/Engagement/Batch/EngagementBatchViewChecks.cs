namespace Acme.Workforce.Contractors.Tests.Engagement.Batch;

using Acme.Workforce.Contractors.Engagement.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class EngagementBatchViewChecks
{
    public static void EngagementBatchViewShowsTheStatusInCapitals()
    {
        var item = EngagementBatch.NewEngagementBatch("code-368", "label-488");
        var view = EngagementBatchView.ToEngagementBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EngagementBatchView.ToEngagementBatchViews(new List<EngagementBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EngagementBatchViewShowsTheStatusInCapitals();
    }
}
