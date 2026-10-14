namespace Acme.Workforce.Assets.Tests.AssetReturn.Adjustment;

using Acme.Workforce.Assets.AssetReturn.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class AssetReturnQuotaChecks
{
    public static void AssetReturnQuotaStartsInPlanned()
    {
        var item = AssetReturnQuota.NewAssetReturnQuota("code-10", "name-668");
        Check.Equal(item.Status, AssetReturnQuotaStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssetReturnQuotaMovesToScheduled()
    {
        var item = AssetReturnQuota.NewAssetReturnQuota("code-10", "name-668");
        try
        {
            item.MoveTo(AssetReturnQuotaStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssetReturnQuotaStatus.Scheduled, "status");
    }

    public static void AssetReturnQuotaRejectsMovesFromCancelled()
    {
        var item = AssetReturnQuota.NewAssetReturnQuota("code-10", "name-668");
        item.Status = AssetReturnQuotaStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AssetReturnQuotaStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AssetReturnQuotaStartsInPlanned();
        AssetReturnQuotaMovesToScheduled();
        AssetReturnQuotaRejectsMovesFromCancelled();
    }
}
