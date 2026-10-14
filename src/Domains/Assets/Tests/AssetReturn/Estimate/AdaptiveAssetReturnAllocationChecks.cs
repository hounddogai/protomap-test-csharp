namespace Acme.Workforce.Assets.Tests.AssetReturn.Estimate;

using Acme.Workforce.Assets.AssetReturn.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class AdaptiveAssetReturnAllocationChecks
{
    public static void AdaptiveAssetReturnAllocationStartsInIdle()
    {
        var item = AdaptiveAssetReturnAllocation.NewAdaptiveAssetReturnAllocation("code-931", "label-759");
        Check.Equal(item.Status, AdaptiveAssetReturnAllocationStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveAssetReturnAllocationMovesToReady()
    {
        var item = AdaptiveAssetReturnAllocation.NewAdaptiveAssetReturnAllocation("code-931", "label-759");
        try
        {
            item.MoveTo(AdaptiveAssetReturnAllocationStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveAssetReturnAllocationStatus.Ready, "status");
    }

    public static void AdaptiveAssetReturnAllocationRejectsMovesFromStopped()
    {
        var item = AdaptiveAssetReturnAllocation.NewAdaptiveAssetReturnAllocation("code-931", "label-759");
        item.Status = AdaptiveAssetReturnAllocationStatus.Stopped;
        Check.Fails(() => item.MoveTo(AdaptiveAssetReturnAllocationStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AdaptiveAssetReturnAllocationStartsInIdle();
        AdaptiveAssetReturnAllocationMovesToReady();
        AdaptiveAssetReturnAllocationRejectsMovesFromStopped();
    }
}
