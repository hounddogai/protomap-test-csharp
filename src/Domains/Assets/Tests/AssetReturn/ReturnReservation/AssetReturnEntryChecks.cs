namespace Acme.Workforce.Assets.Tests.AssetReturn.ReturnReservation;

using Acme.Workforce.Assets.AssetReturn.ReturnReservation;

/// <summary>Checks of return reservation code, run by the project's runner.</summary>
public static class AssetReturnEntryChecks
{
    public static void AssetReturnEntryStartsInOpen()
    {
        var item = AssetReturnEntry.NewAssetReturnEntry("code-284", "title-512");
        Check.Equal(item.Status, AssetReturnEntryStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssetReturnEntryMovesToInReview()
    {
        var item = AssetReturnEntry.NewAssetReturnEntry("code-284", "title-512");
        try
        {
            item.MoveTo(AssetReturnEntryStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssetReturnEntryStatus.InReview, "status");
    }

    public static void AssetReturnEntryRejectsMovesFromArchived()
    {
        var item = AssetReturnEntry.NewAssetReturnEntry("code-284", "title-512");
        item.Status = AssetReturnEntryStatus.Archived;
        Check.Fails(() => item.MoveTo(AssetReturnEntryStatus.Open), "move");
    }

    public static void RunAll()
    {
        AssetReturnEntryStartsInOpen();
        AssetReturnEntryMovesToInReview();
        AssetReturnEntryRejectsMovesFromArchived();
    }
}
