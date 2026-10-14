namespace Acme.Workforce.Assets.Tests.AssetReturn.Variance;

using Acme.Workforce.Assets.AssetReturn.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class AssetReturnVarianceChecks
{
    public static void AssetReturnVarianceStartsInProposed()
    {
        var item = AssetReturnVariance.NewAssetReturnVariance("code-897", "name-784");
        Check.Equal(item.Status, AssetReturnVarianceStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssetReturnVarianceMovesToRejected()
    {
        var item = AssetReturnVariance.NewAssetReturnVariance("code-897", "name-784");
        try
        {
            item.MoveTo(AssetReturnVarianceStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssetReturnVarianceStatus.Rejected, "status");
    }

    public static void AssetReturnVarianceRejectsMovesFromWithdrawn()
    {
        var item = AssetReturnVariance.NewAssetReturnVariance("code-897", "name-784");
        item.Status = AssetReturnVarianceStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(AssetReturnVarianceStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        AssetReturnVarianceStartsInProposed();
        AssetReturnVarianceMovesToRejected();
        AssetReturnVarianceRejectsMovesFromWithdrawn();
    }
}
