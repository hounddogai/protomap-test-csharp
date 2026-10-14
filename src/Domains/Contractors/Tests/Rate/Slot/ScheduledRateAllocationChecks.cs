namespace Acme.Workforce.Contractors.Tests.Rate.Slot;

using Acme.Workforce.Contractors.Rate.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class ScheduledRateAllocationChecks
{
    public static void ScheduledRateAllocationStartsInCreated()
    {
        var item = ScheduledRateAllocation.NewScheduledRateAllocation("code-638", "label-390");
        Check.Equal(item.Status, ScheduledRateAllocationStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledRateAllocationMovesToPublished()
    {
        var item = ScheduledRateAllocation.NewScheduledRateAllocation("code-638", "label-390");
        try
        {
            item.MoveTo(ScheduledRateAllocationStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledRateAllocationStatus.Published, "status");
    }

    public static void ScheduledRateAllocationRejectsMovesFromRetired()
    {
        var item = ScheduledRateAllocation.NewScheduledRateAllocation("code-638", "label-390");
        item.Status = ScheduledRateAllocationStatus.Retired;
        Check.Fails(() => item.MoveTo(ScheduledRateAllocationStatus.Created), "move");
    }

    public static void RunAll()
    {
        ScheduledRateAllocationStartsInCreated();
        ScheduledRateAllocationMovesToPublished();
        ScheduledRateAllocationRejectsMovesFromRetired();
    }
}
