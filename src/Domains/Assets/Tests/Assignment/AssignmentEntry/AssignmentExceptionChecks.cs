namespace Acme.Workforce.Assets.Tests.Assignment.AssignmentEntry;

using Acme.Workforce.Assets.Assignment.AssignmentEntry;

/// <summary>Checks of assignment entry code, run by the project's runner.</summary>
public static class AssignmentExceptionChecks
{
    public static void AssignmentExceptionStartsInProposed()
    {
        var item = AssignmentException.NewAssignmentException("code-612", "title-79");
        Check.Equal(item.Status, AssignmentExceptionStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssignmentExceptionMovesToApproved()
    {
        var item = AssignmentException.NewAssignmentException("code-612", "title-79");
        try
        {
            item.MoveTo(AssignmentExceptionStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssignmentExceptionStatus.Approved, "status");
    }

    public static void AssignmentExceptionRejectsMovesFromWithdrawn()
    {
        var item = AssignmentException.NewAssignmentException("code-612", "title-79");
        item.Status = AssignmentExceptionStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(AssignmentExceptionStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        AssignmentExceptionStartsInProposed();
        AssignmentExceptionMovesToApproved();
        AssignmentExceptionRejectsMovesFromWithdrawn();
    }
}
