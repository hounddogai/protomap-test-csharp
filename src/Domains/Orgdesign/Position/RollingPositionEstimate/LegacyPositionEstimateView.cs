namespace Acme.Workforce.Orgdesign.Position.RollingPositionEstimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy position estimate.</summary>
public sealed record LegacyPositionEstimateView(string Code, string Label, string Budget, string Currency, string Discount, string Rate, string Margin, string StatusLabel)
{
    public static LegacyPositionEstimateView ToLegacyPositionEstimateView(LegacyPositionEstimate item)
    {
        return new LegacyPositionEstimateView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Label, 120),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyPositionEstimateView> ToLegacyPositionEstimateViews(List<LegacyPositionEstimate> items)
    {
        var views = new List<LegacyPositionEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyPositionEstimateView(item));
        }
        return views;
    }

    public static double SumLegacyPositionEstimateView(LegacyPositionEstimateView view)
    {
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return parsedBudget + parsedDiscount + parsedRate;
    }
}
