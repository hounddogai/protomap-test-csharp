namespace Acme.Workforce.Compensation.Tests.PayBand.Baseline;

using Acme.Workforce.Compensation.PayBand.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PayBandSlotChecks
{
    public static void PayBandSlotStartsInRequested()
    {
        var item = PayBandSlot.NewPayBandSlot("code-131", "title-747");
        Check.Equal(item.Status, PayBandSlotStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayBandSlotMovesToRevoked()
    {
        var item = PayBandSlot.NewPayBandSlot("code-131", "title-747");
        try
        {
            item.MoveTo(PayBandSlotStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayBandSlotStatus.Revoked, "status");
    }

    public static void PayBandSlotRejectsMovesFromExpired()
    {
        var item = PayBandSlot.NewPayBandSlot("code-131", "title-747");
        item.Status = PayBandSlotStatus.Expired;
        Check.Fails(() => item.MoveTo(PayBandSlotStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PayBandSlotStartsInRequested();
        PayBandSlotMovesToRevoked();
        PayBandSlotRejectsMovesFromExpired();
    }
}
