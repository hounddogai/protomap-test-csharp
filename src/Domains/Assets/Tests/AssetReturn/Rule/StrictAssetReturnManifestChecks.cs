namespace Acme.Workforce.Assets.Tests.AssetReturn.Rule;

using Acme.Workforce.Assets.AssetReturn.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class StrictAssetReturnManifestChecks
{
    public static void StrictAssetReturnManifestStartsInPlanned()
    {
        var item = StrictAssetReturnManifest.NewStrictAssetReturnManifest("code-173", "name-877");
        Check.Equal(item.Status, StrictAssetReturnManifestStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictAssetReturnManifestMovesToScheduled()
    {
        var item = StrictAssetReturnManifest.NewStrictAssetReturnManifest("code-173", "name-877");
        try
        {
            item.MoveTo(StrictAssetReturnManifestStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictAssetReturnManifestStatus.Scheduled, "status");
    }

    public static void StrictAssetReturnManifestRejectsMovesFromCancelled()
    {
        var item = StrictAssetReturnManifest.NewStrictAssetReturnManifest("code-173", "name-877");
        item.Status = StrictAssetReturnManifestStatus.Cancelled;
        Check.Fails(() => item.MoveTo(StrictAssetReturnManifestStatus.Planned), "move");
    }

    public static void RunAll()
    {
        StrictAssetReturnManifestStartsInPlanned();
        StrictAssetReturnManifestMovesToScheduled();
        StrictAssetReturnManifestRejectsMovesFromCancelled();
    }
}
