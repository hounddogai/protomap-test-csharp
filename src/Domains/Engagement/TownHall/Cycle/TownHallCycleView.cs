namespace Acme.Workforce.Engagement.TownHall.Cycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a town hall cycle.</summary>
public sealed record TownHallCycleView(string Code, string Label, string Fte, string WeightText, string LeaveDays, string Unit, string ExternalRef, string Region, string PinnedText, string StatusLabel)
{
    public static TownHallCycleView ToTownHallCycleView(TownHallCycle item)
    {
        return new TownHallCycleView(
            item.Code,
            Text.Truncate(item.Label, 40),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Unit,
            item.ExternalRef,
            item.Region,
            $"{item.Pinned}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TownHallCycleView> ToTownHallCycleViews(List<TownHallCycle> items)
    {
        var views = new List<TownHallCycleView>();
        foreach (var item in items)
        {
            views.Add(ToTownHallCycleView(item));
        }
        return views;
    }

    public static double SumTownHallCycleView(TownHallCycleView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return parsedFte + parsedWeight + (double)parsedLeaveDays;
    }
}
