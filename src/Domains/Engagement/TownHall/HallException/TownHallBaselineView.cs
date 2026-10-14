namespace Acme.Workforce.Engagement.TownHall.HallException;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a town hall baseline.</summary>
public sealed record TownHallBaselineView(string CodeText, string Name, string LeaveDays, string Archived, string Source, string Threshold, string StatusLabel)
{
    public static TownHallBaselineView ToTownHallBaselineView(TownHallBaseline item)
    {
        return new TownHallBaselineView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Source,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TownHallBaselineView> ToTownHallBaselineViews(List<TownHallBaseline> items)
    {
        var views = new List<TownHallBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToTownHallBaselineView(item));
        }
        return views;
    }

    public static double SumTownHallBaselineView(TownHallBaselineView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + parsedThreshold;
    }
}
