namespace Acme.Workforce.Assets.AssetReturn.Variance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted asset return quota.</summary>
public sealed record WeightedAssetReturnQuotaView(string Code, string TitleText, string Budget, string ApprovedText, string Unit, string ChecksumText, string StatusLabel)
{
    public static WeightedAssetReturnQuotaView ToWeightedAssetReturnQuotaView(WeightedAssetReturnQuota item)
    {
        return new WeightedAssetReturnQuotaView(
            item.Code,
            Text.Truncate(item.Title, 120),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Unit,
            item.Checksum,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedAssetReturnQuotaView> ToWeightedAssetReturnQuotaViews(List<WeightedAssetReturnQuota> items)
    {
        var views = new List<WeightedAssetReturnQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedAssetReturnQuotaView(item));
        }
        return views;
    }

    public static double SumWeightedAssetReturnQuotaView(WeightedAssetReturnQuotaView view)
    {
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return parsedBudget;
    }
}
