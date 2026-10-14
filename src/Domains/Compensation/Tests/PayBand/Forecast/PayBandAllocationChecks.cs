namespace Acme.Workforce.Compensation.Tests.PayBand.Forecast;

using Acme.Workforce.Compensation.PayBand.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class PayBandAllocationChecks
{
    public static void PayBandAllocationStartsInProposed()
    {
        var item = PayBandAllocation.NewPayBandAllocation("code-811", "title-576");
        Check.Equal(item.Status, PayBandAllocationStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayBandAllocationMovesToApproved()
    {
        var item = PayBandAllocation.NewPayBandAllocation("code-811", "title-576");
        try
        {
            item.MoveTo(PayBandAllocationStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayBandAllocationStatus.Approved, "status");
    }

    public static void PayBandAllocationRejectsMovesFromWithdrawn()
    {
        var item = PayBandAllocation.NewPayBandAllocation("code-811", "title-576");
        item.Status = PayBandAllocationStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PayBandAllocationStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PayBandAllocationStartsInProposed();
        PayBandAllocationMovesToApproved();
        PayBandAllocationRejectsMovesFromWithdrawn();
    }
}
