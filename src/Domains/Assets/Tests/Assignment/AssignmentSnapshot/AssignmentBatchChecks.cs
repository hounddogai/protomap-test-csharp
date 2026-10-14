namespace Acme.Workforce.Assets.Tests.Assignment.AssignmentSnapshot;

using Acme.Workforce.Assets.Assignment.AssignmentSnapshot;

/// <summary>Checks of assignment snapshot code, run by the project's runner.</summary>
public static class AssignmentBatchChecks
{
    public static void AssignmentBatchStartsInRequested()
    {
        var item = AssignmentBatch.NewAssignmentBatch("code-51", "name-761");
        Check.Equal(item.Status, AssignmentBatchStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssignmentBatchMovesToRevoked()
    {
        var item = AssignmentBatch.NewAssignmentBatch("code-51", "name-761");
        try
        {
            item.MoveTo(AssignmentBatchStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssignmentBatchStatus.Revoked, "status");
    }

    public static void AssignmentBatchRejectsMovesFromExpired()
    {
        var item = AssignmentBatch.NewAssignmentBatch("code-51", "name-761");
        item.Status = AssignmentBatchStatus.Expired;
        Check.Fails(() => item.MoveTo(AssignmentBatchStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AssignmentBatchStartsInRequested();
        AssignmentBatchMovesToRevoked();
        AssignmentBatchRejectsMovesFromExpired();
    }
}
