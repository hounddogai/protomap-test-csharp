namespace Acme.Workforce.Assets.Tests.AssetReturn.Draft;

using Acme.Workforce.Assets.AssetReturn.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class AssetReturnAllocationChecks
{
    public static void AssetReturnAllocationStartsInDraft()
    {
        var item = AssetReturnAllocation.NewAssetReturnAllocation("code-331", "name-362");
        Check.Equal(item.Status, AssetReturnAllocationStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssetReturnAllocationMovesToPending()
    {
        var item = AssetReturnAllocation.NewAssetReturnAllocation("code-331", "name-362");
        try
        {
            item.MoveTo(AssetReturnAllocationStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssetReturnAllocationStatus.Pending, "status");
    }

    public static void AssetReturnAllocationRejectsMovesFromClosed()
    {
        var item = AssetReturnAllocation.NewAssetReturnAllocation("code-331", "name-362");
        item.Status = AssetReturnAllocationStatus.Closed;
        Check.Fails(() => item.MoveTo(AssetReturnAllocationStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AssetReturnAllocationStartsInDraft();
        AssetReturnAllocationMovesToPending();
        AssetReturnAllocationRejectsMovesFromClosed();
    }
}
