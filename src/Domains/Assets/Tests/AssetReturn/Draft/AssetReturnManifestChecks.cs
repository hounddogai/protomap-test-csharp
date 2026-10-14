namespace Acme.Workforce.Assets.Tests.AssetReturn.Draft;

using Acme.Workforce.Assets.AssetReturn.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class AssetReturnManifestChecks
{
    public static void AssetReturnManifestStartsInPlanned()
    {
        var item = AssetReturnManifest.NewAssetReturnManifest("code-221", "label-400");
        Check.Equal(item.Status, AssetReturnManifestStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssetReturnManifestMovesToScheduled()
    {
        var item = AssetReturnManifest.NewAssetReturnManifest("code-221", "label-400");
        try
        {
            item.MoveTo(AssetReturnManifestStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssetReturnManifestStatus.Scheduled, "status");
    }

    public static void AssetReturnManifestRejectsMovesFromCancelled()
    {
        var item = AssetReturnManifest.NewAssetReturnManifest("code-221", "label-400");
        item.Status = AssetReturnManifestStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AssetReturnManifestStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AssetReturnManifestStartsInPlanned();
        AssetReturnManifestMovesToScheduled();
        AssetReturnManifestRejectsMovesFromCancelled();
    }
}
