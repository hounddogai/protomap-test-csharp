namespace Acme.Workforce.Payrollops.Tests.Garnishment.GarnishmentAllocation;

using Acme.Workforce.Payrollops.Garnishment.GarnishmentAllocation;

/// <summary>Checks of garnishment allocation code, run by the project's runner.</summary>
public static class LegacyGarnishmentAllocationChecks
{
    public static void LegacyGarnishmentAllocationStartsInOpen()
    {
        var item = LegacyGarnishmentAllocation.NewLegacyGarnishmentAllocation("code-324", "label-575");
        Check.Equal(item.Status, LegacyGarnishmentAllocationStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyGarnishmentAllocationMovesToResolved()
    {
        var item = LegacyGarnishmentAllocation.NewLegacyGarnishmentAllocation("code-324", "label-575");
        try
        {
            item.MoveTo(LegacyGarnishmentAllocationStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyGarnishmentAllocationStatus.Resolved, "status");
    }

    public static void LegacyGarnishmentAllocationRejectsMovesFromArchived()
    {
        var item = LegacyGarnishmentAllocation.NewLegacyGarnishmentAllocation("code-324", "label-575");
        item.Status = LegacyGarnishmentAllocationStatus.Archived;
        Check.Fails(() => item.MoveTo(LegacyGarnishmentAllocationStatus.Open), "move");
    }

    public static void RunAll()
    {
        LegacyGarnishmentAllocationStartsInOpen();
        LegacyGarnishmentAllocationMovesToResolved();
        LegacyGarnishmentAllocationRejectsMovesFromArchived();
    }
}
