namespace Acme.Workforce.Assets.Tests.Assignment.Checkpoint;

using Acme.Workforce.Assets.Assignment.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class AssignmentAdjustmentChecks
{
    public static void AssignmentAdjustmentStartsInNew()
    {
        var item = AssignmentAdjustment.NewAssignmentAdjustment("code-369", "name-878");
        Check.Equal(item.Status, AssignmentAdjustmentStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssignmentAdjustmentMovesToQueued()
    {
        var item = AssignmentAdjustment.NewAssignmentAdjustment("code-369", "name-878");
        try
        {
            item.MoveTo(AssignmentAdjustmentStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssignmentAdjustmentStatus.Queued, "status");
    }

    public static void AssignmentAdjustmentRejectsMovesFromFailed()
    {
        var item = AssignmentAdjustment.NewAssignmentAdjustment("code-369", "name-878");
        item.Status = AssignmentAdjustmentStatus.Failed;
        Check.Fails(() => item.MoveTo(AssignmentAdjustmentStatus.New), "move");
    }

    public static void RunAll()
    {
        AssignmentAdjustmentStartsInNew();
        AssignmentAdjustmentMovesToQueued();
        AssignmentAdjustmentRejectsMovesFromFailed();
    }
}
