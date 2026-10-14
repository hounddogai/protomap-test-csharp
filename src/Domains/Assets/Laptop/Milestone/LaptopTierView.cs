namespace Acme.Workforce.Assets.Laptop.Milestone;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a laptop tier.</summary>
public sealed record LaptopTierView(string Code, string Title, string RegionText, string RevisionText, string Ratio, string DurationMinutesText, string Segment, string FloorValue, string LimitText, string StatusLabel)
{
    public static LaptopTierView ToLaptopTierView(LaptopTier item)
    {
        return new LaptopTierView(
            item.Code,
            item.Title,
            item.Region,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Segment, 80),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LaptopTierView> ToLaptopTierViews(List<LaptopTier> items)
    {
        var views = new List<LaptopTierView>();
        foreach (var item in items)
        {
            views.Add(ToLaptopTierView(item));
        }
        return views;
    }

    public static double SumLaptopTierView(LaptopTierView view)
    {
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        return (double)parsedRevision + parsedRatio + (double)parsedDurationMinutes;
    }
}
