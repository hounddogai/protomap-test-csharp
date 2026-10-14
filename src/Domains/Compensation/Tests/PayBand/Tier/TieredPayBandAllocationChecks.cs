namespace Acme.Workforce.Compensation.Tests.PayBand.Tier;

using Acme.Workforce.Compensation.PayBand.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class TieredPayBandAllocationChecks
{
    public static void TieredPayBandAllocationStartsInOpen()
    {
        var item = TieredPayBandAllocation.NewTieredPayBandAllocation("code-198", "name-59");
        Check.Equal(item.Status, TieredPayBandAllocationStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredPayBandAllocationMovesToResolved()
    {
        var item = TieredPayBandAllocation.NewTieredPayBandAllocation("code-198", "name-59");
        try
        {
            item.MoveTo(TieredPayBandAllocationStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredPayBandAllocationStatus.Resolved, "status");
    }

    public static void TieredPayBandAllocationRejectsMovesFromArchived()
    {
        var item = TieredPayBandAllocation.NewTieredPayBandAllocation("code-198", "name-59");
        item.Status = TieredPayBandAllocationStatus.Archived;
        Check.Fails(() => item.MoveTo(TieredPayBandAllocationStatus.Open), "move");
    }

    public static void RunAll()
    {
        TieredPayBandAllocationStartsInOpen();
        TieredPayBandAllocationMovesToResolved();
        TieredPayBandAllocationRejectsMovesFromArchived();
    }
}
