namespace Acme.Workforce.Assets.Tests.AssetReturn.Profile;

using Acme.Workforce.Assets.AssetReturn.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class AssetReturnProfileChecks
{
    public static void AssetReturnProfileStartsInProposed()
    {
        var item = AssetReturnProfile.NewAssetReturnProfile("code-443", "title-797");
        Check.Equal(item.Status, AssetReturnProfileStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssetReturnProfileMovesToApproved()
    {
        var item = AssetReturnProfile.NewAssetReturnProfile("code-443", "title-797");
        try
        {
            item.MoveTo(AssetReturnProfileStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssetReturnProfileStatus.Approved, "status");
    }

    public static void AssetReturnProfileRejectsMovesFromWithdrawn()
    {
        var item = AssetReturnProfile.NewAssetReturnProfile("code-443", "title-797");
        item.Status = AssetReturnProfileStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(AssetReturnProfileStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        AssetReturnProfileStartsInProposed();
        AssetReturnProfileMovesToApproved();
        AssetReturnProfileRejectsMovesFromWithdrawn();
    }
}
