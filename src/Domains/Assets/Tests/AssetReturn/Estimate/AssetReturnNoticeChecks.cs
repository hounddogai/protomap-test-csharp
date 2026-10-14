namespace Acme.Workforce.Assets.Tests.AssetReturn.Estimate;

using Acme.Workforce.Assets.AssetReturn.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class AssetReturnNoticeChecks
{
    public static void AssetReturnNoticeStartsInCreated()
    {
        var item = AssetReturnNotice.NewAssetReturnNotice("code-62", "title-429");
        Check.Equal(item.Status, AssetReturnNoticeStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssetReturnNoticeMovesToRetired()
    {
        var item = AssetReturnNotice.NewAssetReturnNotice("code-62", "title-429");
        try
        {
            item.MoveTo(AssetReturnNoticeStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssetReturnNoticeStatus.Retired, "status");
    }

    public static void AssetReturnNoticeRejectsMovesFromRetired()
    {
        var item = AssetReturnNotice.NewAssetReturnNotice("code-62", "title-429");
        item.Status = AssetReturnNoticeStatus.Retired;
        Check.Fails(() => item.MoveTo(AssetReturnNoticeStatus.Created), "move");
    }

    public static void RunAll()
    {
        AssetReturnNoticeStartsInCreated();
        AssetReturnNoticeMovesToRetired();
        AssetReturnNoticeRejectsMovesFromRetired();
    }
}
