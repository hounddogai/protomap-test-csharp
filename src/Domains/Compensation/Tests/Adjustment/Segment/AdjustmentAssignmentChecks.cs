namespace Acme.Workforce.Compensation.Tests.Adjustment.Segment;

using Acme.Workforce.Compensation.Adjustment.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class AdjustmentAssignmentChecks
{
    public static void AdjustmentAssignmentStartsInIdle()
    {
        var item = AdjustmentAssignment.NewAdjustmentAssignment("code-431", "name-979");
        Check.Equal(item.Status, AdjustmentAssignmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdjustmentAssignmentMovesToWarming()
    {
        var item = AdjustmentAssignment.NewAdjustmentAssignment("code-431", "name-979");
        try
        {
            item.MoveTo(AdjustmentAssignmentStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdjustmentAssignmentStatus.Warming, "status");
    }

    public static void AdjustmentAssignmentRejectsMovesFromStopped()
    {
        var item = AdjustmentAssignment.NewAdjustmentAssignment("code-431", "name-979");
        item.Status = AdjustmentAssignmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(AdjustmentAssignmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AdjustmentAssignmentStartsInIdle();
        AdjustmentAssignmentMovesToWarming();
        AdjustmentAssignmentRejectsMovesFromStopped();
    }
}
