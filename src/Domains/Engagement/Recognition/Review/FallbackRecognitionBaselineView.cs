namespace Acme.Workforce.Engagement.Recognition.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback recognition baseline.</summary>
public sealed record FallbackRecognitionBaselineView(string Code, string Name, string Unit, string Cost, string Location, string Badge, string StartsAt, string Channel, string Ratio, string Capacity, string StatusLabel)
{
    public static FallbackRecognitionBaselineView ToFallbackRecognitionBaselineView(FallbackRecognitionBaseline item)
    {
        return new FallbackRecognitionBaselineView(
            item.Code,
            item.Name,
            item.Unit,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Location,
            Text.Truncate(item.Badge, 120),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Channel, 120),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackRecognitionBaselineView> ToFallbackRecognitionBaselineViews(List<FallbackRecognitionBaseline> items)
    {
        var views = new List<FallbackRecognitionBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackRecognitionBaselineView(item));
        }
        return views;
    }

    public static double SumFallbackRecognitionBaselineView(FallbackRecognitionBaselineView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return parsedCost + (double)parsedStartsAt + parsedRatio;
    }
}
