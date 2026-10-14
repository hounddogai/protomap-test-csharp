namespace Acme.Workforce.Assets.Tests.AssetReturn.ReturnReservation;

using Acme.Workforce.Assets.AssetReturn.ReturnReservation;

/// <summary>Checks of return reservation code, run by the project's runner.</summary>
public static class PrimaryAssetReturnReservationChecks
{
    public static void PrimaryAssetReturnReservationStartsInOpen()
    {
        var item = PrimaryAssetReturnReservation.NewPrimaryAssetReturnReservation("code-909", "title-352");
        Check.Equal(item.Status, PrimaryAssetReturnReservationStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryAssetReturnReservationMovesToInReview()
    {
        var item = PrimaryAssetReturnReservation.NewPrimaryAssetReturnReservation("code-909", "title-352");
        try
        {
            item.MoveTo(PrimaryAssetReturnReservationStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryAssetReturnReservationStatus.InReview, "status");
    }

    public static void PrimaryAssetReturnReservationRejectsMovesFromArchived()
    {
        var item = PrimaryAssetReturnReservation.NewPrimaryAssetReturnReservation("code-909", "title-352");
        item.Status = PrimaryAssetReturnReservationStatus.Archived;
        Check.Fails(() => item.MoveTo(PrimaryAssetReturnReservationStatus.Open), "move");
    }

    public static void RunAll()
    {
        PrimaryAssetReturnReservationStartsInOpen();
        PrimaryAssetReturnReservationMovesToInReview();
        PrimaryAssetReturnReservationRejectsMovesFromArchived();
    }
}
