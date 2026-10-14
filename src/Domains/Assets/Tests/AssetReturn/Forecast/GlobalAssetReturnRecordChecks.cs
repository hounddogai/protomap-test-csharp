namespace Acme.Workforce.Assets.Tests.AssetReturn.Forecast;

using Acme.Workforce.Assets.AssetReturn.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class GlobalAssetReturnRecordChecks
{
    public static void GlobalAssetReturnRecordStartsInProposed()
    {
        var item = GlobalAssetReturnRecord.NewGlobalAssetReturnRecord("code-611", "name-658");
        Check.Equal(item.Status, GlobalAssetReturnRecordStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalAssetReturnRecordMovesToWithdrawn()
    {
        var item = GlobalAssetReturnRecord.NewGlobalAssetReturnRecord("code-611", "name-658");
        try
        {
            item.MoveTo(GlobalAssetReturnRecordStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalAssetReturnRecordStatus.Withdrawn, "status");
    }

    public static void GlobalAssetReturnRecordRejectsMovesFromWithdrawn()
    {
        var item = GlobalAssetReturnRecord.NewGlobalAssetReturnRecord("code-611", "name-658");
        item.Status = GlobalAssetReturnRecordStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(GlobalAssetReturnRecordStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        GlobalAssetReturnRecordStartsInProposed();
        GlobalAssetReturnRecordMovesToWithdrawn();
        GlobalAssetReturnRecordRejectsMovesFromWithdrawn();
    }
}
