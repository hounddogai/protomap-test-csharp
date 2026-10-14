namespace Acme.Workforce.Orgdesign.Position.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a position threshold.</summary>
public sealed record PositionThresholdView(string Code, string Label, string ExternalRef, string Discount, string Location, string Limit, string Rating, string Attempts, string AmountText, string StatusLabel)
{
    public static PositionThresholdView ToPositionThresholdView(PositionThreshold item)
    {
        return new PositionThresholdView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Label, 80),
            item.ExternalRef,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Location,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PositionThresholdView> ToPositionThresholdViews(List<PositionThreshold> items)
    {
        var views = new List<PositionThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToPositionThresholdView(item));
        }
        return views;
    }

    public static double SumPositionThresholdView(PositionThresholdView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return parsedDiscount + (double)parsedLimit + parsedRating;
    }
}
