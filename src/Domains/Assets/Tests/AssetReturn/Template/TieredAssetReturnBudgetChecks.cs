namespace Acme.Workforce.Assets.Tests.AssetReturn.Template;

using Acme.Workforce.Assets.AssetReturn.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class TieredAssetReturnBudgetChecks
{
    public static void TieredAssetReturnBudgetStartsInPlanned()
    {
        var item = TieredAssetReturnBudget.NewTieredAssetReturnBudget("code-77", "title-924");
        Check.Equal(item.Status, TieredAssetReturnBudgetStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredAssetReturnBudgetMovesToInProgress()
    {
        var item = TieredAssetReturnBudget.NewTieredAssetReturnBudget("code-77", "title-924");
        try
        {
            item.MoveTo(TieredAssetReturnBudgetStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredAssetReturnBudgetStatus.InProgress, "status");
    }

    public static void TieredAssetReturnBudgetRejectsMovesFromCancelled()
    {
        var item = TieredAssetReturnBudget.NewTieredAssetReturnBudget("code-77", "title-924");
        item.Status = TieredAssetReturnBudgetStatus.Cancelled;
        Check.Fails(() => item.MoveTo(TieredAssetReturnBudgetStatus.Planned), "move");
    }

    public static void RunAll()
    {
        TieredAssetReturnBudgetStartsInPlanned();
        TieredAssetReturnBudgetMovesToInProgress();
        TieredAssetReturnBudgetRejectsMovesFromCancelled();
    }
}
