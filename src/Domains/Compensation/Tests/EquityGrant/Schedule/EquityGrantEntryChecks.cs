namespace Acme.Workforce.Compensation.Tests.EquityGrant.Schedule;

using Acme.Workforce.Compensation.EquityGrant.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class EquityGrantEntryChecks
{
    public static void EquityGrantEntryStartsInIdle()
    {
        var item = EquityGrantEntry.NewEquityGrantEntry("code-165", "label-722");
        Check.Equal(item.Status, EquityGrantEntryStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EquityGrantEntryMovesToDraining()
    {
        var item = EquityGrantEntry.NewEquityGrantEntry("code-165", "label-722");
        try
        {
            item.MoveTo(EquityGrantEntryStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EquityGrantEntryStatus.Draining, "status");
    }

    public static void EquityGrantEntryRejectsMovesFromStopped()
    {
        var item = EquityGrantEntry.NewEquityGrantEntry("code-165", "label-722");
        item.Status = EquityGrantEntryStatus.Stopped;
        Check.Fails(() => item.MoveTo(EquityGrantEntryStatus.Idle), "move");
    }

    public static void RunAll()
    {
        EquityGrantEntryStartsInIdle();
        EquityGrantEntryMovesToDraining();
        EquityGrantEntryRejectsMovesFromStopped();
    }
}
