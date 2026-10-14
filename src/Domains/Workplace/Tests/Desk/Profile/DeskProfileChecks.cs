namespace Acme.Workforce.Workplace.Tests.Desk.Profile;

using Acme.Workforce.Workplace.Desk.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class DeskProfileChecks
{
    public static void DeskProfileStartsInPlanned()
    {
        var item = DeskProfile.NewDeskProfile("code-620", "name-930");
        Check.Equal(item.Status, DeskProfileStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeskProfileMovesToScheduled()
    {
        var item = DeskProfile.NewDeskProfile("code-620", "name-930");
        try
        {
            item.MoveTo(DeskProfileStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeskProfileStatus.Scheduled, "status");
    }

    public static void DeskProfileRejectsMovesFromCancelled()
    {
        var item = DeskProfile.NewDeskProfile("code-620", "name-930");
        item.Status = DeskProfileStatus.Cancelled;
        Check.Fails(() => item.MoveTo(DeskProfileStatus.Planned), "move");
    }

    public static void RunAll()
    {
        DeskProfileStartsInPlanned();
        DeskProfileMovesToScheduled();
        DeskProfileRejectsMovesFromCancelled();
    }
}
