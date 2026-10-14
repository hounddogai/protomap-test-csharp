namespace Acme.Workforce.Compensation.Tests.Adjustment.Segment;

using Acme.Workforce.Compensation.Adjustment.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class AdjustmentAssignmentViewChecks
{
    public static void AdjustmentAssignmentViewShowsTheStatusInCapitals()
    {
        var item = AdjustmentAssignment.NewAdjustmentAssignment("code-431", "name-979");
        var view = AdjustmentAssignmentView.ToAdjustmentAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdjustmentAssignmentView.ToAdjustmentAssignmentViews(new List<AdjustmentAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdjustmentAssignmentViewShowsTheStatusInCapitals();
    }
}
