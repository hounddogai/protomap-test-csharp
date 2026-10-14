namespace Acme.Workforce.Workplace.Occupancy.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional occupancy digest.</summary>
public sealed record RegionalOccupancyDigestView(string Code, string Title, string Channel, string Archived, string Weight, string Priority, string Sequence, string Badge, string StatusLabel)
{
    public static RegionalOccupancyDigestView ToRegionalOccupancyDigestView(RegionalOccupancyDigest item)
    {
        return new RegionalOccupancyDigestView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Title, 40),
            item.Channel,
            $"{item.Archived}",
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Badge, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalOccupancyDigestView> ToRegionalOccupancyDigestViews(List<RegionalOccupancyDigest> items)
    {
        var views = new List<RegionalOccupancyDigestView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalOccupancyDigestView(item));
        }
        return views;
    }

    public static double SumRegionalOccupancyDigestView(RegionalOccupancyDigestView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return parsedWeight + (double)parsedPriority + (double)parsedSequence;
    }
}
