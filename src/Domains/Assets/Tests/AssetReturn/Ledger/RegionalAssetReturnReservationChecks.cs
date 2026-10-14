namespace Acme.Workforce.Assets.Tests.AssetReturn.Ledger;

using Acme.Workforce.Assets.AssetReturn.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class RegionalAssetReturnReservationChecks
{
    public static void RegionalAssetReturnReservationStartsInCreated()
    {
        var item = RegionalAssetReturnReservation.NewRegionalAssetReturnReservation("code-951", "title-942");
        Check.Equal(item.Status, RegionalAssetReturnReservationStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalAssetReturnReservationMovesToPublished()
    {
        var item = RegionalAssetReturnReservation.NewRegionalAssetReturnReservation("code-951", "title-942");
        try
        {
            item.MoveTo(RegionalAssetReturnReservationStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalAssetReturnReservationStatus.Published, "status");
    }

    public static void RegionalAssetReturnReservationRejectsMovesFromRetired()
    {
        var item = RegionalAssetReturnReservation.NewRegionalAssetReturnReservation("code-951", "title-942");
        item.Status = RegionalAssetReturnReservationStatus.Retired;
        Check.Fails(() => item.MoveTo(RegionalAssetReturnReservationStatus.Created), "move");
    }

    public static void RunAll()
    {
        RegionalAssetReturnReservationStartsInCreated();
        RegionalAssetReturnReservationMovesToPublished();
        RegionalAssetReturnReservationRejectsMovesFromRetired();
    }
}
