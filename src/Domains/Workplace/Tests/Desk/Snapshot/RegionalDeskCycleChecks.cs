namespace Acme.Workforce.Workplace.Tests.Desk.Snapshot;

using Acme.Workforce.Workplace.Desk.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class RegionalDeskCycleChecks
{
    public static void RegionalDeskCycleStartsInNew()
    {
        var item = RegionalDeskCycle.NewRegionalDeskCycle("code-763", "name-503");
        Check.Equal(item.Status, RegionalDeskCycleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalDeskCycleMovesToRunning()
    {
        var item = RegionalDeskCycle.NewRegionalDeskCycle("code-763", "name-503");
        try
        {
            item.MoveTo(RegionalDeskCycleStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalDeskCycleStatus.Running, "status");
    }

    public static void RegionalDeskCycleRejectsMovesFromFailed()
    {
        var item = RegionalDeskCycle.NewRegionalDeskCycle("code-763", "name-503");
        item.Status = RegionalDeskCycleStatus.Failed;
        Check.Fails(() => item.MoveTo(RegionalDeskCycleStatus.New), "move");
    }

    public static void RunAll()
    {
        RegionalDeskCycleStartsInNew();
        RegionalDeskCycleMovesToRunning();
        RegionalDeskCycleRejectsMovesFromFailed();
    }
}
