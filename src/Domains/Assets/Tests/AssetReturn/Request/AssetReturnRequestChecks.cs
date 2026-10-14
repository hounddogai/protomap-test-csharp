namespace Acme.Workforce.Assets.Tests.AssetReturn.Request;

using Acme.Workforce.Assets.AssetReturn.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class AssetReturnRequestChecks
{
    public static void AssetReturnRequestStartsInRequested()
    {
        var item = AssetReturnRequest.NewAssetReturnRequest("code-367", "title-885");
        Check.Equal(item.Status, AssetReturnRequestStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssetReturnRequestMovesToRevoked()
    {
        var item = AssetReturnRequest.NewAssetReturnRequest("code-367", "title-885");
        try
        {
            item.MoveTo(AssetReturnRequestStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssetReturnRequestStatus.Revoked, "status");
    }

    public static void AssetReturnRequestRejectsMovesFromExpired()
    {
        var item = AssetReturnRequest.NewAssetReturnRequest("code-367", "title-885");
        item.Status = AssetReturnRequestStatus.Expired;
        Check.Fails(() => item.MoveTo(AssetReturnRequestStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AssetReturnRequestStartsInRequested();
        AssetReturnRequestMovesToRevoked();
        AssetReturnRequestRejectsMovesFromExpired();
    }
}
