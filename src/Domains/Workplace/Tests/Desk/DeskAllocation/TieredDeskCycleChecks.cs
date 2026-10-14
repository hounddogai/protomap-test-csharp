namespace Acme.Workforce.Workplace.Tests.Desk.DeskAllocation;

using Acme.Workforce.Workplace.Desk.DeskAllocation;

/// <summary>Checks of desk allocation code, run by the project's runner.</summary>
public static class TieredDeskCycleChecks
{
    public static void TieredDeskCycleStartsInCreated()
    {
        var item = TieredDeskCycle.NewTieredDeskCycle("code-234", "label-385");
        Check.Equal(item.Status, TieredDeskCycleStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredDeskCycleMovesToRetired()
    {
        var item = TieredDeskCycle.NewTieredDeskCycle("code-234", "label-385");
        try
        {
            item.MoveTo(TieredDeskCycleStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredDeskCycleStatus.Retired, "status");
    }

    public static void TieredDeskCycleRejectsMovesFromRetired()
    {
        var item = TieredDeskCycle.NewTieredDeskCycle("code-234", "label-385");
        item.Status = TieredDeskCycleStatus.Retired;
        Check.Fails(() => item.MoveTo(TieredDeskCycleStatus.Created), "move");
    }

    public static void RunAll()
    {
        TieredDeskCycleStartsInCreated();
        TieredDeskCycleMovesToRetired();
        TieredDeskCycleRejectsMovesFromRetired();
    }
}
