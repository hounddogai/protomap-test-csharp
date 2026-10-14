namespace Acme.Workforce.Engagement.TownHall.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a town hall tier.</summary>
public sealed record TownHallTierView(string Code, string Label, string Margin, string EndsAt, string Capacity, string Team, string StatusLabel)
{
    public static TownHallTierView ToTownHallTierView(TownHallTier item)
    {
        return new TownHallTierView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Team,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TownHallTierView> ToTownHallTierViews(List<TownHallTier> items)
    {
        var views = new List<TownHallTierView>();
        foreach (var item in items)
        {
            views.Add(ToTownHallTierView(item));
        }
        return views;
    }

    public static double SumTownHallTierView(TownHallTierView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return parsedMargin + (double)parsedEndsAt + (double)parsedCapacity;
    }
}
