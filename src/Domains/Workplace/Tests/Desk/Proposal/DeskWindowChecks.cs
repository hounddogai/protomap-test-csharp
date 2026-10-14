namespace Acme.Workforce.Workplace.Tests.Desk.Proposal;

using Acme.Workforce.Workplace.Desk.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DeskWindowChecks
{
    public static void DeskWindowStartsInDraft()
    {
        var item = DeskWindow.NewDeskWindow("code-598", "name-602");
        Check.Equal(item.Status, DeskWindowStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeskWindowMovesToPending()
    {
        var item = DeskWindow.NewDeskWindow("code-598", "name-602");
        try
        {
            item.MoveTo(DeskWindowStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeskWindowStatus.Pending, "status");
    }

    public static void DeskWindowRejectsMovesFromClosed()
    {
        var item = DeskWindow.NewDeskWindow("code-598", "name-602");
        item.Status = DeskWindowStatus.Closed;
        Check.Fails(() => item.MoveTo(DeskWindowStatus.Draft), "move");
    }

    public static void RunAll()
    {
        DeskWindowStartsInDraft();
        DeskWindowMovesToPending();
        DeskWindowRejectsMovesFromClosed();
    }
}
