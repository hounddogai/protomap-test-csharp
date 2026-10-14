namespace Acme.Workforce.Contractors.Tests.Rate.Batch;

using Acme.Workforce.Contractors.Rate.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class RateAllocationChecks
{
    public static void RateAllocationStartsInIdle()
    {
        var item = RateAllocation.NewRateAllocation("code-947", "label-582");
        Check.Equal(item.Status, RateAllocationStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RateAllocationMovesToWarming()
    {
        var item = RateAllocation.NewRateAllocation("code-947", "label-582");
        try
        {
            item.MoveTo(RateAllocationStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RateAllocationStatus.Warming, "status");
    }

    public static void RateAllocationRejectsMovesFromStopped()
    {
        var item = RateAllocation.NewRateAllocation("code-947", "label-582");
        item.Status = RateAllocationStatus.Stopped;
        Check.Fails(() => item.MoveTo(RateAllocationStatus.Idle), "move");
    }

    public static void RunAll()
    {
        RateAllocationStartsInIdle();
        RateAllocationMovesToWarming();
        RateAllocationRejectsMovesFromStopped();
    }
}
