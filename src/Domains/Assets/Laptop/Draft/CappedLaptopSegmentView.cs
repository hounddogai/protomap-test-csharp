namespace Acme.Workforce.Assets.Laptop.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped laptop segment.</summary>
public sealed record CappedLaptopSegmentView(string Code, string Label, string Visible, string Hours, string Ratio, string Channel, string CountryText, string Category, string RatingText, string Cost, string StatusLabel)
{
    public static CappedLaptopSegmentView ToCappedLaptopSegmentView(CappedLaptopSegment item)
    {
        return new CappedLaptopSegmentView(
            item.Code,
            item.Label,
            $"{item.Visible}",
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Channel, 120),
            Text.Truncate(item.Country, 80),
            item.Category,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedLaptopSegmentView> ToCappedLaptopSegmentViews(List<CappedLaptopSegment> items)
    {
        var views = new List<CappedLaptopSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToCappedLaptopSegmentView(item));
        }
        return views;
    }

    public static double SumCappedLaptopSegmentView(CappedLaptopSegmentView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        return parsedHours + parsedRatio + parsedRating;
    }
}
