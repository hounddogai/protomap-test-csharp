namespace Acme.Workforce.Assets.Tests.AssetReturn.Reservation;

using Acme.Workforce.Assets.AssetReturn.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class AssetReturnReservationChecks
{
    public static void AssetReturnReservationStartsInOpen()
    {
        var item = AssetReturnReservation.NewAssetReturnReservation("code-286", "label-900");
        Check.Equal(item.Status, AssetReturnReservationStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssetReturnReservationMovesToInReview()
    {
        var item = AssetReturnReservation.NewAssetReturnReservation("code-286", "label-900");
        try
        {
            item.MoveTo(AssetReturnReservationStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssetReturnReservationStatus.InReview, "status");
    }

    public static void AssetReturnReservationRejectsMovesFromArchived()
    {
        var item = AssetReturnReservation.NewAssetReturnReservation("code-286", "label-900");
        item.Status = AssetReturnReservationStatus.Archived;
        Check.Fails(() => item.MoveTo(AssetReturnReservationStatus.Open), "move");
    }

    public static void RunAll()
    {
        AssetReturnReservationStartsInOpen();
        AssetReturnReservationMovesToInReview();
        AssetReturnReservationRejectsMovesFromArchived();
    }
}
