namespace Acme.Workforce.Engagement.TownHall.HallException;

using System.Globalization;

/// <summary>How screens and exports show a town hall preference.</summary>
public sealed record TownHallPreferenceView(string Code, string Label, string StartsAt, string HoursText, string Rate, string Score, string Maximum, string Approved, string DurationMinutes, string UpdatedAt, string Name, string StatusLabel)
{
    public static TownHallPreferenceView ToTownHallPreferenceView(TownHallPreference item)
    {
        return new TownHallPreferenceView(
            item.Code,
            item.Label,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Name,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TownHallPreferenceView> ToTownHallPreferenceViews(List<TownHallPreference> items)
    {
        var views = new List<TownHallPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToTownHallPreferenceView(item));
        }
        return views;
    }

    public static double SumTownHallPreferenceView(TownHallPreferenceView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + parsedHours + parsedRate;
    }
}
