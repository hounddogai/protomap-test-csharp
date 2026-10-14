namespace Acme.Workforce.Workplace.Tests.Desk.DeskProposal;

using Acme.Workforce.Workplace.Desk.DeskProposal;

/// <summary>Checks of desk proposal code, run by the project's runner.</summary>
public static class DeskExceptionChecks
{
    public static void DeskExceptionStartsInProposed()
    {
        var item = DeskException.NewDeskException("code-206", "title-474");
        Check.Equal(item.Status, DeskExceptionStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeskExceptionMovesToApproved()
    {
        var item = DeskException.NewDeskException("code-206", "title-474");
        try
        {
            item.MoveTo(DeskExceptionStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeskExceptionStatus.Approved, "status");
    }

    public static void DeskExceptionRejectsMovesFromWithdrawn()
    {
        var item = DeskException.NewDeskException("code-206", "title-474");
        item.Status = DeskExceptionStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(DeskExceptionStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        DeskExceptionStartsInProposed();
        DeskExceptionMovesToApproved();
        DeskExceptionRejectsMovesFromWithdrawn();
    }
}
