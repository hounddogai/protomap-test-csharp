namespace Acme.Workforce.Payrollops.Tests.Garnishment.Allocation;

using Acme.Workforce.Payrollops.Garnishment.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class GarnishmentAllocationChecks
{
    public static void GarnishmentAllocationStartsInCreated()
    {
        var item = GarnishmentAllocation.NewGarnishmentAllocation("code-196", "title-532");
        Check.Equal(item.Status, GarnishmentAllocationStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GarnishmentAllocationMovesToPublished()
    {
        var item = GarnishmentAllocation.NewGarnishmentAllocation("code-196", "title-532");
        try
        {
            item.MoveTo(GarnishmentAllocationStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GarnishmentAllocationStatus.Published, "status");
    }

    public static void GarnishmentAllocationRejectsMovesFromRetired()
    {
        var item = GarnishmentAllocation.NewGarnishmentAllocation("code-196", "title-532");
        item.Status = GarnishmentAllocationStatus.Retired;
        Check.Fails(() => item.MoveTo(GarnishmentAllocationStatus.Created), "move");
    }

    public static void RunAll()
    {
        GarnishmentAllocationStartsInCreated();
        GarnishmentAllocationMovesToPublished();
        GarnishmentAllocationRejectsMovesFromRetired();
    }
}
