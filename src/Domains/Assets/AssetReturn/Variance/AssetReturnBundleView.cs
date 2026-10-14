namespace Acme.Workforce.Assets.AssetReturn.Variance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a asset return bundle.</summary>
public sealed record AssetReturnBundleView(string Code, string Name, string CostCenter, string BatchSize, string Department, string Budget, string Skill, string GraceDays, string Active, string Cost, string StatusLabel)
{
    public static AssetReturnBundleView ToAssetReturnBundleView(AssetReturnBundle item)
    {
        return new AssetReturnBundleView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.CostCenter,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Department,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AssetReturnBundleView> ToAssetReturnBundleViews(List<AssetReturnBundle> items)
    {
        var views = new List<AssetReturnBundleView>();
        foreach (var item in items)
        {
            views.Add(ToAssetReturnBundleView(item));
        }
        return views;
    }

    public static double SumAssetReturnBundleView(AssetReturnBundleView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + parsedBudget + (double)parsedGraceDays;
    }
}
