namespace Acme.Workforce.Assets.Tests.Assignment.Batch;

using Acme.Workforce.Assets.Assignment.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class DefaultAssignmentBatchViewChecks
{
    public static void DefaultAssignmentBatchViewShowsTheStatusInCapitals()
    {
        var item = DefaultAssignmentBatch.NewDefaultAssignmentBatch("code-872", "name-195");
        var view = DefaultAssignmentBatchView.ToDefaultAssignmentBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultAssignmentBatchView.ToDefaultAssignmentBatchViews(new List<DefaultAssignmentBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultAssignmentBatchViewShowsTheStatusInCapitals();
    }
}
