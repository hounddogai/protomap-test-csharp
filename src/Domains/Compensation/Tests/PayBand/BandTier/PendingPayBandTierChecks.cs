namespace Acme.Workforce.Compensation.Tests.PayBand.BandTier;

using Acme.Workforce.Compensation.PayBand.BandTier;

/// <summary>Checks of band tier code, run by the project's runner.</summary>
public static class PendingPayBandTierChecks
{
    public static void PendingPayBandTierStartsInCreated()
    {
        var item = PendingPayBandTier.NewPendingPayBandTier("code-131", "label-985");
        Check.Equal(item.Status, PendingPayBandTierStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingPayBandTierMovesToValidated()
    {
        var item = PendingPayBandTier.NewPendingPayBandTier("code-131", "label-985");
        try
        {
            item.MoveTo(PendingPayBandTierStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingPayBandTierStatus.Validated, "status");
    }

    public static void PendingPayBandTierRejectsMovesFromRetired()
    {
        var item = PendingPayBandTier.NewPendingPayBandTier("code-131", "label-985");
        item.Status = PendingPayBandTierStatus.Retired;
        Check.Fails(() => item.MoveTo(PendingPayBandTierStatus.Created), "move");
    }

    public static void RunAll()
    {
        PendingPayBandTierStartsInCreated();
        PendingPayBandTierMovesToValidated();
        PendingPayBandTierRejectsMovesFromRetired();
    }
}
