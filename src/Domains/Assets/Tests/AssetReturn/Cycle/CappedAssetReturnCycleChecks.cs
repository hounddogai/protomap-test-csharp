namespace Acme.Workforce.Assets.Tests.AssetReturn.Cycle;

using Acme.Workforce.Assets.AssetReturn.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class CappedAssetReturnCycleChecks
{
    public static void CappedAssetReturnCycleStartsInIdle()
    {
        var item = CappedAssetReturnCycle.NewCappedAssetReturnCycle("code-567", "name-357");
        Check.Equal(item.Status, CappedAssetReturnCycleStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedAssetReturnCycleMovesToStopped()
    {
        var item = CappedAssetReturnCycle.NewCappedAssetReturnCycle("code-567", "name-357");
        try
        {
            item.MoveTo(CappedAssetReturnCycleStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedAssetReturnCycleStatus.Stopped, "status");
    }

    public static void CappedAssetReturnCycleRejectsMovesFromStopped()
    {
        var item = CappedAssetReturnCycle.NewCappedAssetReturnCycle("code-567", "name-357");
        item.Status = CappedAssetReturnCycleStatus.Stopped;
        Check.Fails(() => item.MoveTo(CappedAssetReturnCycleStatus.Idle), "move");
    }

    public static void RunAll()
    {
        CappedAssetReturnCycleStartsInIdle();
        CappedAssetReturnCycleMovesToStopped();
        CappedAssetReturnCycleRejectsMovesFromStopped();
    }
}
