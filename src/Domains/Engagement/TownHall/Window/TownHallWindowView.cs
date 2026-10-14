namespace Acme.Workforce.Engagement.TownHall.Window;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a town hall window.</summary>
public sealed record TownHallWindowView(string Code, string Title, string EndsAt, string Margin, string Active, string Segment, string Source, string GraceDaysText, string Team, string Reviewer, string StatusLabel)
{
    public static TownHallWindowView ToTownHallWindowView(TownHallWindow item)
    {
        return new TownHallWindowView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Title, 120),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.Segment,
            item.Source,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Team, 64),
            item.Reviewer,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TownHallWindowView> ToTownHallWindowViews(List<TownHallWindow> items)
    {
        var views = new List<TownHallWindowView>();
        foreach (var item in items)
        {
            views.Add(ToTownHallWindowView(item));
        }
        return views;
    }

    public static double SumTownHallWindowView(TownHallWindowView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + parsedMargin + (double)parsedGraceDays;
    }
}
