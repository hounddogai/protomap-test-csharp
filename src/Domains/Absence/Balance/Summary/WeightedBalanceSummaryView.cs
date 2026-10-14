namespace Acme.Workforce.Absence.Balance.Summary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted balance summary.</summary>
public sealed record WeightedBalanceSummaryView(string Code, string Label, string Grade, string ThresholdText, string DescriptionText, string Amount, string SalaryBandText, string StatusLabel)
{
    public static WeightedBalanceSummaryView ToWeightedBalanceSummaryView(WeightedBalanceSummary item)
    {
        return new WeightedBalanceSummaryView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.Grade,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Description,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.SalaryBand,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedBalanceSummaryView> ToWeightedBalanceSummaryViews(List<WeightedBalanceSummary> items)
    {
        var views = new List<WeightedBalanceSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedBalanceSummaryView(item));
        }
        return views;
    }

    public static double SumWeightedBalanceSummaryView(WeightedBalanceSummaryView view)
    {
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedThreshold + parsedAmount;
    }
}
