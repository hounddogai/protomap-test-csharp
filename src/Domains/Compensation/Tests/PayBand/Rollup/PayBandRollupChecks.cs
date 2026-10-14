namespace Acme.Workforce.Compensation.Tests.PayBand.Rollup;

using Acme.Workforce.Compensation.PayBand.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class PayBandRollupChecks
{
    public static void PayBandRollupStartsInProposed()
    {
        var item = PayBandRollup.NewPayBandRollup("code-635", "title-282");
        Check.Equal(item.Status, PayBandRollupStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayBandRollupMovesToApproved()
    {
        var item = PayBandRollup.NewPayBandRollup("code-635", "title-282");
        try
        {
            item.MoveTo(PayBandRollupStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayBandRollupStatus.Approved, "status");
    }

    public static void PayBandRollupRejectsMovesFromWithdrawn()
    {
        var item = PayBandRollup.NewPayBandRollup("code-635", "title-282");
        item.Status = PayBandRollupStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PayBandRollupStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PayBandRollupStartsInProposed();
        PayBandRollupMovesToApproved();
        PayBandRollupRejectsMovesFromWithdrawn();
    }
}
