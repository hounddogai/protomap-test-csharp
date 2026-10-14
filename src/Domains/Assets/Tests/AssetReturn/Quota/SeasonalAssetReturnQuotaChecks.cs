namespace Acme.Workforce.Assets.Tests.AssetReturn.Quota;

using Acme.Workforce.Assets.AssetReturn.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class SeasonalAssetReturnQuotaChecks
{
    public static void SeasonalAssetReturnQuotaStartsInNew()
    {
        var item = SeasonalAssetReturnQuota.NewSeasonalAssetReturnQuota("code-96", "title-556");
        Check.Equal(item.Status, SeasonalAssetReturnQuotaStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalAssetReturnQuotaMovesToFailed()
    {
        var item = SeasonalAssetReturnQuota.NewSeasonalAssetReturnQuota("code-96", "title-556");
        try
        {
            item.MoveTo(SeasonalAssetReturnQuotaStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalAssetReturnQuotaStatus.Failed, "status");
    }

    public static void SeasonalAssetReturnQuotaRejectsMovesFromFailed()
    {
        var item = SeasonalAssetReturnQuota.NewSeasonalAssetReturnQuota("code-96", "title-556");
        item.Status = SeasonalAssetReturnQuotaStatus.Failed;
        Check.Fails(() => item.MoveTo(SeasonalAssetReturnQuotaStatus.New), "move");
    }

    public static void RunAll()
    {
        SeasonalAssetReturnQuotaStartsInNew();
        SeasonalAssetReturnQuotaMovesToFailed();
        SeasonalAssetReturnQuotaRejectsMovesFromFailed();
    }
}
