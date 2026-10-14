namespace Acme.Workforce.Assets.Tests.LicenseSeat.Bundle;

using Acme.Workforce.Assets.LicenseSeat.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class LicenseSeatAllocationChecks
{
    public static void LicenseSeatAllocationStartsInPlanned()
    {
        var item = LicenseSeatAllocation.NewLicenseSeatAllocation("code-550", "label-156");
        Check.Equal(item.Status, LicenseSeatAllocationStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LicenseSeatAllocationMovesToCompleted()
    {
        var item = LicenseSeatAllocation.NewLicenseSeatAllocation("code-550", "label-156");
        try
        {
            item.MoveTo(LicenseSeatAllocationStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LicenseSeatAllocationStatus.Completed, "status");
    }

    public static void LicenseSeatAllocationRejectsMovesFromCancelled()
    {
        var item = LicenseSeatAllocation.NewLicenseSeatAllocation("code-550", "label-156");
        item.Status = LicenseSeatAllocationStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LicenseSeatAllocationStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LicenseSeatAllocationStartsInPlanned();
        LicenseSeatAllocationMovesToCompleted();
        LicenseSeatAllocationRejectsMovesFromCancelled();
    }
}
