namespace Acme.Workforce.Absence.Tests.ReturnPlan.Bundle;

using Acme.Workforce.Absence.ReturnPlan.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class ReturnPlanCheckpointChecks
{
    public static void ReturnPlanCheckpointStartsInProposed()
    {
        var item = ReturnPlanCheckpoint.NewReturnPlanCheckpoint("code-55", "title-866");
        Check.Equal(item.Status, ReturnPlanCheckpointStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReturnPlanCheckpointMovesToApproved()
    {
        var item = ReturnPlanCheckpoint.NewReturnPlanCheckpoint("code-55", "title-866");
        try
        {
            item.MoveTo(ReturnPlanCheckpointStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReturnPlanCheckpointStatus.Approved, "status");
    }

    public static void ReturnPlanCheckpointRejectsMovesFromWithdrawn()
    {
        var item = ReturnPlanCheckpoint.NewReturnPlanCheckpoint("code-55", "title-866");
        item.Status = ReturnPlanCheckpointStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ReturnPlanCheckpointStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ReturnPlanCheckpointStartsInProposed();
        ReturnPlanCheckpointMovesToApproved();
        ReturnPlanCheckpointRejectsMovesFromWithdrawn();
    }
}
