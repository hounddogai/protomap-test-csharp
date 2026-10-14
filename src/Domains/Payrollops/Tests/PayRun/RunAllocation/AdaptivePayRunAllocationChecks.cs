namespace Acme.Workforce.Payrollops.Tests.PayRun.RunAllocation;

using Acme.Workforce.Payrollops.PayRun.RunAllocation;

/// <summary>Checks of run allocation code, run by the project's runner.</summary>
public static class AdaptivePayRunAllocationChecks
{
    public static void AdaptivePayRunAllocationStartsInCreated()
    {
        var item = AdaptivePayRunAllocation.NewAdaptivePayRunAllocation("code-524", "title-322");
        Check.Equal(item.Status, AdaptivePayRunAllocationStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptivePayRunAllocationMovesToPublished()
    {
        var item = AdaptivePayRunAllocation.NewAdaptivePayRunAllocation("code-524", "title-322");
        try
        {
            item.MoveTo(AdaptivePayRunAllocationStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptivePayRunAllocationStatus.Published, "status");
    }

    public static void AdaptivePayRunAllocationRejectsMovesFromRetired()
    {
        var item = AdaptivePayRunAllocation.NewAdaptivePayRunAllocation("code-524", "title-322");
        item.Status = AdaptivePayRunAllocationStatus.Retired;
        Check.Fails(() => item.MoveTo(AdaptivePayRunAllocationStatus.Created), "move");
    }

    public static void RunAll()
    {
        AdaptivePayRunAllocationStartsInCreated();
        AdaptivePayRunAllocationMovesToPublished();
        AdaptivePayRunAllocationRejectsMovesFromRetired();
    }
}
