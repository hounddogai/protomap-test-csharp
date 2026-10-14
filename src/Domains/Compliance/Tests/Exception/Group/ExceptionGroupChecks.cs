namespace Acme.Workforce.Compliance.Tests.Exception.Group;

using Acme.Workforce.Compliance.Exception.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ExceptionGroupChecks
{
    public static void ExceptionGroupStartsInProposed()
    {
        var item = ExceptionGroup.NewExceptionGroup("code-762", "title-45");
        Check.Equal(item.Status, ExceptionGroupStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ExceptionGroupMovesToRejected()
    {
        var item = ExceptionGroup.NewExceptionGroup("code-762", "title-45");
        try
        {
            item.MoveTo(ExceptionGroupStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ExceptionGroupStatus.Rejected, "status");
    }

    public static void ExceptionGroupRejectsMovesFromWithdrawn()
    {
        var item = ExceptionGroup.NewExceptionGroup("code-762", "title-45");
        item.Status = ExceptionGroupStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ExceptionGroupStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ExceptionGroupStartsInProposed();
        ExceptionGroupMovesToRejected();
        ExceptionGroupRejectsMovesFromWithdrawn();
    }
}
