namespace Acme.Workforce.Assets.AssetReturn.Estimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a asset return estimate.</summary>
public sealed record AssetReturnEstimateView(string Code, string TitleText, string BudgetText, string FteText, string BadgeText, string VisibleText, string ChannelText, string ExternalRefText, string DescriptionText, string StatusLabel)
{
    public static AssetReturnEstimateView ToAssetReturnEstimateView(AssetReturnEstimate item)
    {
        return new AssetReturnEstimateView(
            Text.Truncate(item.Code, 64),
            item.Title,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            $"{item.Visible}",
            item.Channel,
            item.ExternalRef,
            Text.Truncate(item.Description, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AssetReturnEstimateView> ToAssetReturnEstimateViews(List<AssetReturnEstimate> items)
    {
        var views = new List<AssetReturnEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToAssetReturnEstimateView(item));
        }
        return views;
    }

    public static double SumAssetReturnEstimateView(AssetReturnEstimateView view)
    {
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        return parsedBudget + parsedFte;
    }
}
