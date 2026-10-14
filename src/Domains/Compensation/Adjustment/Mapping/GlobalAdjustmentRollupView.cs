namespace Acme.Workforce.Compensation.Adjustment.Mapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global adjustment rollup.</summary>
public sealed record GlobalAdjustmentRollupView(string CodeText, string Name, string ThresholdText, string Amount, string Hours, string SalaryBandText, string Checksum, string Timezone, string DiscountText, string CreatedAt, string StatusLabel)
{
    public static GlobalAdjustmentRollupView ToGlobalAdjustmentRollupView(GlobalAdjustmentRollup item)
    {
        return new GlobalAdjustmentRollupView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.SalaryBand,
            Text.Truncate(item.Checksum, 120),
            Text.Truncate(item.Timezone, 64),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalAdjustmentRollupView> ToGlobalAdjustmentRollupViews(List<GlobalAdjustmentRollup> items)
    {
        var views = new List<GlobalAdjustmentRollupView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalAdjustmentRollupView(item));
        }
        return views;
    }

    public static double SumGlobalAdjustmentRollupView(GlobalAdjustmentRollupView view)
    {
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return parsedThreshold + parsedAmount + parsedHours;
    }
}
