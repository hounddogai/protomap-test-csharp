namespace Acme.Workforce.Assets.Tests.Warranty.Digest;

using Acme.Workforce.Assets.Warranty.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class LenientWarrantyForecastChecks
{
    public static void LenientWarrantyForecastStartsInProposed()
    {
        var item = LenientWarrantyForecast.NewLenientWarrantyForecast("code-675", "label-486");
        Check.Equal(item.Status, LenientWarrantyForecastStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientWarrantyForecastMovesToApproved()
    {
        var item = LenientWarrantyForecast.NewLenientWarrantyForecast("code-675", "label-486");
        try
        {
            item.MoveTo(LenientWarrantyForecastStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientWarrantyForecastStatus.Approved, "status");
    }

    public static void LenientWarrantyForecastRejectsMovesFromWithdrawn()
    {
        var item = LenientWarrantyForecast.NewLenientWarrantyForecast("code-675", "label-486");
        item.Status = LenientWarrantyForecastStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LenientWarrantyForecastStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LenientWarrantyForecastStartsInProposed();
        LenientWarrantyForecastMovesToApproved();
        LenientWarrantyForecastRejectsMovesFromWithdrawn();
    }
}
