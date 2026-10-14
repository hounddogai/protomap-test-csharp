namespace Acme.Workforce.Assets.AssetReturn.Cycle;

using System.Globalization;

/// <summary>How screens and exports show a lenient asset return adjustment.</summary>
public sealed record LenientAssetReturnAdjustmentView(string CodeText, string TitleText, string Notes, string Limit, string Discount, string PriorityText, string CreatedAtText, string Ratio, string Threshold, string CeilingText, string StatusLabel)
{
    public static LenientAssetReturnAdjustmentView ToLenientAssetReturnAdjustmentView(LenientAssetReturnAdjustment item)
    {
        return new LenientAssetReturnAdjustmentView(
            item.Code,
            item.Title,
            item.Notes,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientAssetReturnAdjustmentView> ToLenientAssetReturnAdjustmentViews(List<LenientAssetReturnAdjustment> items)
    {
        var views = new List<LenientAssetReturnAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToLenientAssetReturnAdjustmentView(item));
        }
        return views;
    }

    public static double SumLenientAssetReturnAdjustmentView(LenientAssetReturnAdjustmentView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        return (double)parsedLimit + parsedDiscount + (double)parsedPriority;
    }
}
