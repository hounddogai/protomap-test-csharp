namespace Acme.Workforce.Assets.Tests.AssetReturn.Template;

using Acme.Workforce.Assets.AssetReturn.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class TieredAssetReturnBudgetViewChecks
{
    public static void TieredAssetReturnBudgetViewShowsTheStatusInCapitals()
    {
        var item = TieredAssetReturnBudget.NewTieredAssetReturnBudget("code-77", "title-924");
        var view = TieredAssetReturnBudgetView.ToTieredAssetReturnBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredAssetReturnBudgetView.ToTieredAssetReturnBudgetViews(new List<TieredAssetReturnBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredAssetReturnBudgetViewShowsTheStatusInCapitals();
    }
}
