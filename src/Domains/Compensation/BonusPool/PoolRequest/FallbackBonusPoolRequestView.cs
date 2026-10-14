namespace Acme.Workforce.Compensation.BonusPool.PoolRequest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback bonus pool request.</summary>
public sealed record FallbackBonusPoolRequestView(string CodeText, string Name, string Department, string Rating, string Margin, string Label, string QuantityText, string Attempts, string Segment, string Priority, string StatusLabel)
{
    public static FallbackBonusPoolRequestView ToFallbackBonusPoolRequestView(FallbackBonusPoolRequest item)
    {
        return new FallbackBonusPoolRequestView(
            item.Code,
            item.Name,
            Text.Truncate(item.Department, 120),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Label,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Segment, 64),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackBonusPoolRequestView> ToFallbackBonusPoolRequestViews(List<FallbackBonusPoolRequest> items)
    {
        var views = new List<FallbackBonusPoolRequestView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackBonusPoolRequestView(item));
        }
        return views;
    }

    public static double SumFallbackBonusPoolRequestView(FallbackBonusPoolRequestView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        return parsedRating + parsedMargin + (double)parsedQuantity;
    }
}
