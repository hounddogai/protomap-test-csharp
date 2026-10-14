namespace Acme.Workforce.Assets.Tests.AssetReturn.Reservation;

using Acme.Workforce.Assets.AssetReturn.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class AssetReturnThresholdChecks
{
    public static void AssetReturnThresholdStartsInIdle()
    {
        var item = AssetReturnThreshold.NewAssetReturnThreshold("code-958", "label-737");
        Check.Equal(item.Status, AssetReturnThresholdStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssetReturnThresholdMovesToReady()
    {
        var item = AssetReturnThreshold.NewAssetReturnThreshold("code-958", "label-737");
        try
        {
            item.MoveTo(AssetReturnThresholdStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssetReturnThresholdStatus.Ready, "status");
    }

    public static void AssetReturnThresholdRejectsMovesFromStopped()
    {
        var item = AssetReturnThreshold.NewAssetReturnThreshold("code-958", "label-737");
        item.Status = AssetReturnThresholdStatus.Stopped;
        Check.Fails(() => item.MoveTo(AssetReturnThresholdStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AssetReturnThresholdStartsInIdle();
        AssetReturnThresholdMovesToReady();
        AssetReturnThresholdRejectsMovesFromStopped();
    }
}
