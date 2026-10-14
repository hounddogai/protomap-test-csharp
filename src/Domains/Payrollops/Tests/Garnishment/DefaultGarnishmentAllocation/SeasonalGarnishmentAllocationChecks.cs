namespace Acme.Workforce.Payrollops.Tests.Garnishment.DefaultGarnishmentAllocation;

using Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

/// <summary>Checks of default garnishment allocation code, run by the project's runner.</summary>
public static class SeasonalGarnishmentAllocationChecks
{
    public static void SeasonalGarnishmentAllocationStartsInPlanned()
    {
        var item = SeasonalGarnishmentAllocation.NewSeasonalGarnishmentAllocation("code-406", "title-465");
        Check.Equal(item.Status, SeasonalGarnishmentAllocationStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalGarnishmentAllocationMovesToScheduled()
    {
        var item = SeasonalGarnishmentAllocation.NewSeasonalGarnishmentAllocation("code-406", "title-465");
        try
        {
            item.MoveTo(SeasonalGarnishmentAllocationStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalGarnishmentAllocationStatus.Scheduled, "status");
    }

    public static void SeasonalGarnishmentAllocationRejectsMovesFromCancelled()
    {
        var item = SeasonalGarnishmentAllocation.NewSeasonalGarnishmentAllocation("code-406", "title-465");
        item.Status = SeasonalGarnishmentAllocationStatus.Cancelled;
        Check.Fails(() => item.MoveTo(SeasonalGarnishmentAllocationStatus.Planned), "move");
    }

    public static void RunAll()
    {
        SeasonalGarnishmentAllocationStartsInPlanned();
        SeasonalGarnishmentAllocationMovesToScheduled();
        SeasonalGarnishmentAllocationRejectsMovesFromCancelled();
    }
}
