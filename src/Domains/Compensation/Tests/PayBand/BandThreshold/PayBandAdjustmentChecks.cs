namespace Acme.Workforce.Compensation.Tests.PayBand.BandThreshold;

using Acme.Workforce.Compensation.PayBand.BandThreshold;

/// <summary>Checks of band threshold code, run by the project's runner.</summary>
public static class PayBandAdjustmentChecks
{
    public static void PayBandAdjustmentStartsInRequested()
    {
        var item = PayBandAdjustment.NewPayBandAdjustment("code-156", "label-798");
        Check.Equal(item.Status, PayBandAdjustmentStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayBandAdjustmentMovesToRevoked()
    {
        var item = PayBandAdjustment.NewPayBandAdjustment("code-156", "label-798");
        try
        {
            item.MoveTo(PayBandAdjustmentStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayBandAdjustmentStatus.Revoked, "status");
    }

    public static void PayBandAdjustmentRejectsMovesFromExpired()
    {
        var item = PayBandAdjustment.NewPayBandAdjustment("code-156", "label-798");
        item.Status = PayBandAdjustmentStatus.Expired;
        Check.Fails(() => item.MoveTo(PayBandAdjustmentStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PayBandAdjustmentStartsInRequested();
        PayBandAdjustmentMovesToRevoked();
        PayBandAdjustmentRejectsMovesFromExpired();
    }
}
