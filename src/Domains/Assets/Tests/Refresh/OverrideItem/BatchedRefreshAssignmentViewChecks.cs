namespace Acme.Workforce.Assets.Tests.Refresh.OverrideItem;

using Acme.Workforce.Assets.Refresh.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class BatchedRefreshAssignmentViewChecks
{
    public static void BatchedRefreshAssignmentViewShowsTheStatusInCapitals()
    {
        var item = BatchedRefreshAssignment.NewBatchedRefreshAssignment("code-903", "name-999");
        var view = BatchedRefreshAssignmentView.ToBatchedRefreshAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedRefreshAssignmentView.ToBatchedRefreshAssignmentViews(new List<BatchedRefreshAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedRefreshAssignmentViewShowsTheStatusInCapitals();
    }
}
