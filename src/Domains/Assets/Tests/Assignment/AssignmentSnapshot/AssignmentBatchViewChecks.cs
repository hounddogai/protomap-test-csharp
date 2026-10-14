namespace Acme.Workforce.Assets.Tests.Assignment.AssignmentSnapshot;

using Acme.Workforce.Assets.Assignment.AssignmentSnapshot;

/// <summary>Checks of assignment snapshot code, run by the project's runner.</summary>
public static class AssignmentBatchViewChecks
{
    public static void AssignmentBatchViewShowsTheStatusInCapitals()
    {
        var item = AssignmentBatch.NewAssignmentBatch("code-51", "name-761");
        var view = AssignmentBatchView.ToAssignmentBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AssignmentBatchView.ToAssignmentBatchViews(new List<AssignmentBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AssignmentBatchViewShowsTheStatusInCapitals();
    }
}
