namespace Acme.Workforce.Workplace.Tests.Desk.DeskProfile;

using Acme.Workforce.Workplace.Desk.DeskProfile;

/// <summary>Checks of desk profile code, run by the project's runner.</summary>
public static class ScheduledDeskRollupChecks
{
    public static void ScheduledDeskRollupStartsInNew()
    {
        var item = ScheduledDeskRollup.NewScheduledDeskRollup("code-540", "name-182");
        Check.Equal(item.Status, ScheduledDeskRollupStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledDeskRollupMovesToRunning()
    {
        var item = ScheduledDeskRollup.NewScheduledDeskRollup("code-540", "name-182");
        try
        {
            item.MoveTo(ScheduledDeskRollupStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledDeskRollupStatus.Running, "status");
    }

    public static void ScheduledDeskRollupRejectsMovesFromFailed()
    {
        var item = ScheduledDeskRollup.NewScheduledDeskRollup("code-540", "name-182");
        item.Status = ScheduledDeskRollupStatus.Failed;
        Check.Fails(() => item.MoveTo(ScheduledDeskRollupStatus.New), "move");
    }

    public static void RunAll()
    {
        ScheduledDeskRollupStartsInNew();
        ScheduledDeskRollupMovesToRunning();
        ScheduledDeskRollupRejectsMovesFromFailed();
    }
}
