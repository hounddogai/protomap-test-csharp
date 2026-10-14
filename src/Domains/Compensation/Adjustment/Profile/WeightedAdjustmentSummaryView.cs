namespace Acme.Workforce.Compensation.Adjustment.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted adjustment summary.</summary>
public sealed record WeightedAdjustmentSummaryView(string Code, string LabelText, string LimitText, string Name, string Score, string Ceiling, string ChecksumText, string StatusLabel)
{
    public static WeightedAdjustmentSummaryView ToWeightedAdjustmentSummaryView(WeightedAdjustmentSummary item)
    {
        return new WeightedAdjustmentSummaryView(
            item.Code,
            Text.Truncate(item.Label, 40),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Name,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Checksum, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedAdjustmentSummaryView> ToWeightedAdjustmentSummaryViews(List<WeightedAdjustmentSummary> items)
    {
        var views = new List<WeightedAdjustmentSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedAdjustmentSummaryView(item));
        }
        return views;
    }

    public static double SumWeightedAdjustmentSummaryView(WeightedAdjustmentSummaryView view)
    {
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return (double)parsedLimit + parsedScore + parsedCeiling;
    }
}
