namespace Acme.Workforce.Orgdesign.Reorg.Threshold;

using System.Globalization;

/// <summary>How screens and exports show a reorg threshold.</summary>
public sealed record ReorgThresholdView(string Code, string Title, string SalaryBandText, string Reviewer, string Discount, string ExternalRefText, string CostText, string Channel, string StatusLabel)
{
    public static ReorgThresholdView ToReorgThresholdView(ReorgThreshold item)
    {
        return new ReorgThresholdView(
            item.Code,
            item.Title,
            item.SalaryBand,
            item.Reviewer,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Channel,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReorgThresholdView> ToReorgThresholdViews(List<ReorgThreshold> items)
    {
        var views = new List<ReorgThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToReorgThresholdView(item));
        }
        return views;
    }

    public static double SumReorgThresholdView(ReorgThresholdView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        return parsedDiscount + parsedCost;
    }
}
