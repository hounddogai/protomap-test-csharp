namespace Acme.Workforce.Assets.Tests.AssetReturn.Segment;

using Acme.Workforce.Assets.AssetReturn.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class DefaultAssetReturnEstimateChecks
{
    public static void DefaultAssetReturnEstimateStartsInIdle()
    {
        var item = DefaultAssetReturnEstimate.NewDefaultAssetReturnEstimate("code-600", "title-992");
        Check.Equal(item.Status, DefaultAssetReturnEstimateStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultAssetReturnEstimateMovesToWarming()
    {
        var item = DefaultAssetReturnEstimate.NewDefaultAssetReturnEstimate("code-600", "title-992");
        try
        {
            item.MoveTo(DefaultAssetReturnEstimateStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultAssetReturnEstimateStatus.Warming, "status");
    }

    public static void DefaultAssetReturnEstimateRejectsMovesFromStopped()
    {
        var item = DefaultAssetReturnEstimate.NewDefaultAssetReturnEstimate("code-600", "title-992");
        item.Status = DefaultAssetReturnEstimateStatus.Stopped;
        Check.Fails(() => item.MoveTo(DefaultAssetReturnEstimateStatus.Idle), "move");
    }

    public static void RunAll()
    {
        DefaultAssetReturnEstimateStartsInIdle();
        DefaultAssetReturnEstimateMovesToWarming();
        DefaultAssetReturnEstimateRejectsMovesFromStopped();
    }
}
