namespace Acme.Workforce.Engagement.TownHall.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a town hall bundle.</summary>
public sealed record TownHallBundleView(string CodeText, string Name, string Score, string Grade, string SegmentText, string Pinned, string Discount, string Approved, string Ratio, string Location, string Locale, string StatusLabel)
{
    public static TownHallBundleView ToTownHallBundleView(TownHallBundle item)
    {
        return new TownHallBundleView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Grade, 40),
            item.Segment,
            $"{item.Pinned}",
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Location,
            item.Locale,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TownHallBundleView> ToTownHallBundleViews(List<TownHallBundle> items)
    {
        var views = new List<TownHallBundleView>();
        foreach (var item in items)
        {
            views.Add(ToTownHallBundleView(item));
        }
        return views;
    }

    public static double SumTownHallBundleView(TownHallBundleView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return parsedScore + parsedDiscount + parsedRatio;
    }
}
