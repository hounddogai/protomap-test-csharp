namespace Acme.Workforce.Workplace.Occupancy.Digest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy occupancy digest.</summary>
public sealed record LegacyOccupancyDigestView(string Code, string Name, string DurationMinutes, string Margin, string Rate, string Rating, string UpdatedAtText, string ExternalRefText, string FloorValue, string Currency, string StatusLabel)
{
    public static LegacyOccupancyDigestView ToLegacyOccupancyDigestView(LegacyOccupancyDigest item)
    {
        return new LegacyOccupancyDigestView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Currency, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyOccupancyDigestView> ToLegacyOccupancyDigestViews(List<LegacyOccupancyDigest> items)
    {
        var views = new List<LegacyOccupancyDigestView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyOccupancyDigestView(item));
        }
        return views;
    }

    public static double SumLegacyOccupancyDigestView(LegacyOccupancyDigestView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + parsedMargin + parsedRate;
    }
}
