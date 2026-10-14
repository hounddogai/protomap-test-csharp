namespace Acme.Workforce.Assets.AssetReturn.Rule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive asset return variance.</summary>
public sealed record AdaptiveAssetReturnVarianceView(string CodeText, string NameText, string LeaveDays, string CostText, string CreatedAt, string Quantity, string DescriptionText, string Ceiling, string SalaryBand, string Archived, string StatusLabel)
{
    public static AdaptiveAssetReturnVarianceView ToAdaptiveAssetReturnVarianceView(AdaptiveAssetReturnVariance item)
    {
        return new AdaptiveAssetReturnVarianceView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Description,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.SalaryBand,
            $"{item.Archived}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveAssetReturnVarianceView> ToAdaptiveAssetReturnVarianceViews(List<AdaptiveAssetReturnVariance> items)
    {
        var views = new List<AdaptiveAssetReturnVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveAssetReturnVarianceView(item));
        }
        return views;
    }

    public static double SumAdaptiveAssetReturnVarianceView(AdaptiveAssetReturnVarianceView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + parsedCost + (double)parsedCreatedAt;
    }
}
