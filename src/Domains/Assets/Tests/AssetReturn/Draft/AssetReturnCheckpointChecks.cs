namespace Acme.Workforce.Assets.Tests.AssetReturn.Draft;

using Acme.Workforce.Assets.AssetReturn.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class AssetReturnCheckpointChecks
{
    public static void AssetReturnCheckpointStartsInDraft()
    {
        var item = AssetReturnCheckpoint.NewAssetReturnCheckpoint("code-45", "name-320");
        Check.Equal(item.Status, AssetReturnCheckpointStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssetReturnCheckpointMovesToActive()
    {
        var item = AssetReturnCheckpoint.NewAssetReturnCheckpoint("code-45", "name-320");
        try
        {
            item.MoveTo(AssetReturnCheckpointStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssetReturnCheckpointStatus.Active, "status");
    }

    public static void AssetReturnCheckpointRejectsMovesFromClosed()
    {
        var item = AssetReturnCheckpoint.NewAssetReturnCheckpoint("code-45", "name-320");
        item.Status = AssetReturnCheckpointStatus.Closed;
        Check.Fails(() => item.MoveTo(AssetReturnCheckpointStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AssetReturnCheckpointStartsInDraft();
        AssetReturnCheckpointMovesToActive();
        AssetReturnCheckpointRejectsMovesFromClosed();
    }
}
