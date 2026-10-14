namespace Acme.Workforce.Payrollops.Tests.Garnishment.Mapping;

using Acme.Workforce.Payrollops.Garnishment.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class AdaptiveGarnishmentAllocationChecks
{
    public static void AdaptiveGarnishmentAllocationStartsInIdle()
    {
        var item = AdaptiveGarnishmentAllocation.NewAdaptiveGarnishmentAllocation("code-445", "title-727");
        Check.Equal(item.Status, AdaptiveGarnishmentAllocationStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveGarnishmentAllocationMovesToWarming()
    {
        var item = AdaptiveGarnishmentAllocation.NewAdaptiveGarnishmentAllocation("code-445", "title-727");
        try
        {
            item.MoveTo(AdaptiveGarnishmentAllocationStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveGarnishmentAllocationStatus.Warming, "status");
    }

    public static void AdaptiveGarnishmentAllocationRejectsMovesFromStopped()
    {
        var item = AdaptiveGarnishmentAllocation.NewAdaptiveGarnishmentAllocation("code-445", "title-727");
        item.Status = AdaptiveGarnishmentAllocationStatus.Stopped;
        Check.Fails(() => item.MoveTo(AdaptiveGarnishmentAllocationStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AdaptiveGarnishmentAllocationStartsInIdle();
        AdaptiveGarnishmentAllocationMovesToWarming();
        AdaptiveGarnishmentAllocationRejectsMovesFromStopped();
    }
}
