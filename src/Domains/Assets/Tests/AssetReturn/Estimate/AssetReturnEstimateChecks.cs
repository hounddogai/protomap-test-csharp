namespace Acme.Workforce.Assets.Tests.AssetReturn.Estimate;

using Acme.Workforce.Assets.AssetReturn.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class AssetReturnEstimateChecks
{
    public static void AssetReturnEstimateStartsInIdle()
    {
        var item = AssetReturnEstimate.NewAssetReturnEstimate("code-296", "title-603");
        Check.Equal(item.Status, AssetReturnEstimateStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssetReturnEstimateMovesToWarming()
    {
        var item = AssetReturnEstimate.NewAssetReturnEstimate("code-296", "title-603");
        try
        {
            item.MoveTo(AssetReturnEstimateStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssetReturnEstimateStatus.Warming, "status");
    }

    public static void AssetReturnEstimateRejectsMovesFromStopped()
    {
        var item = AssetReturnEstimate.NewAssetReturnEstimate("code-296", "title-603");
        item.Status = AssetReturnEstimateStatus.Stopped;
        Check.Fails(() => item.MoveTo(AssetReturnEstimateStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AssetReturnEstimateStartsInIdle();
        AssetReturnEstimateMovesToWarming();
        AssetReturnEstimateRejectsMovesFromStopped();
    }
}
