namespace Acme.Workforce.Compensation.Tests.PayBand.Baseline;

using Acme.Workforce.Compensation.PayBand.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class GlobalPayBandAllocationChecks
{
    public static void GlobalPayBandAllocationStartsInPlanned()
    {
        var item = GlobalPayBandAllocation.NewGlobalPayBandAllocation("code-682", "title-244");
        Check.Equal(item.Status, GlobalPayBandAllocationStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalPayBandAllocationMovesToScheduled()
    {
        var item = GlobalPayBandAllocation.NewGlobalPayBandAllocation("code-682", "title-244");
        try
        {
            item.MoveTo(GlobalPayBandAllocationStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalPayBandAllocationStatus.Scheduled, "status");
    }

    public static void GlobalPayBandAllocationRejectsMovesFromCancelled()
    {
        var item = GlobalPayBandAllocation.NewGlobalPayBandAllocation("code-682", "title-244");
        item.Status = GlobalPayBandAllocationStatus.Cancelled;
        Check.Fails(() => item.MoveTo(GlobalPayBandAllocationStatus.Planned), "move");
    }

    public static void RunAll()
    {
        GlobalPayBandAllocationStartsInPlanned();
        GlobalPayBandAllocationMovesToScheduled();
        GlobalPayBandAllocationRejectsMovesFromCancelled();
    }
}
