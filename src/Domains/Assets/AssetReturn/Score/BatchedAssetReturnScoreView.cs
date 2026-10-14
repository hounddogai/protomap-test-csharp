namespace Acme.Workforce.Assets.AssetReturn.Score;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched asset return score.</summary>
public sealed record BatchedAssetReturnScoreView(string Code, string NameText, string Budget, string Region, string Archived, string StatusLabel)
{
    public static BatchedAssetReturnScoreView ToBatchedAssetReturnScoreView(BatchedAssetReturnScore item)
    {
        return new BatchedAssetReturnScoreView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Region,
            $"{item.Archived}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedAssetReturnScoreView> ToBatchedAssetReturnScoreViews(List<BatchedAssetReturnScore> items)
    {
        var views = new List<BatchedAssetReturnScoreView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedAssetReturnScoreView(item));
        }
        return views;
    }

    public static double SumBatchedAssetReturnScoreView(BatchedAssetReturnScoreView view)
    {
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return parsedBudget;
    }
}
