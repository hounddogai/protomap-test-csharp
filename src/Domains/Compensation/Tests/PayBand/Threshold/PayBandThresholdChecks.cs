namespace Acme.Workforce.Compensation.Tests.PayBand.Threshold;

using Acme.Workforce.Compensation.PayBand.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PayBandThresholdChecks
{
    public static void PayBandThresholdStartsInProposed()
    {
        var item = PayBandThreshold.NewPayBandThreshold("code-921", "label-526");
        Check.Equal(item.Status, PayBandThresholdStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayBandThresholdMovesToRejected()
    {
        var item = PayBandThreshold.NewPayBandThreshold("code-921", "label-526");
        try
        {
            item.MoveTo(PayBandThresholdStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayBandThresholdStatus.Rejected, "status");
    }

    public static void PayBandThresholdRejectsMovesFromWithdrawn()
    {
        var item = PayBandThreshold.NewPayBandThreshold("code-921", "label-526");
        item.Status = PayBandThresholdStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PayBandThresholdStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PayBandThresholdStartsInProposed();
        PayBandThresholdMovesToRejected();
        PayBandThresholdRejectsMovesFromWithdrawn();
    }
}
