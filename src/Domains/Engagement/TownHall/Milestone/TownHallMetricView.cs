namespace Acme.Workforce.Engagement.TownHall.Milestone;

using System.Globalization;

/// <summary>How screens and exports show a town hall metric.</summary>
public sealed record TownHallMetricView(string Code, string Title, string Location, string Name, string MaximumText, string CreatedAtText, string StartsAtText, string Active, string Score, string Sequence, string Weight, string StatusLabel)
{
    public static TownHallMetricView ToTownHallMetricView(TownHallMetric item)
    {
        return new TownHallMetricView(
            item.Code,
            item.Title,
            item.Location,
            item.Name,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TownHallMetricView> ToTownHallMetricViews(List<TownHallMetric> items)
    {
        var views = new List<TownHallMetricView>();
        foreach (var item in items)
        {
            views.Add(ToTownHallMetricView(item));
        }
        return views;
    }

    public static double SumTownHallMetricView(TownHallMetricView view)
    {
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        return parsedMaximum + (double)parsedCreatedAt + (double)parsedStartsAt;
    }
}
