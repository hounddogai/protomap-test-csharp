namespace Acme.Workforce.Engagement.TownHall.Adjustment;

using System.Globalization;

/// <summary>How screens and exports show a seasonal town hall cycle.</summary>
public sealed record SeasonalTownHallCycleView(string CodeText, string Label, string LimitText, string CountryText, string WindowDays, string GraceDays, string StatusLabel)
{
    public static SeasonalTownHallCycleView ToSeasonalTownHallCycleView(SeasonalTownHallCycle item)
    {
        return new SeasonalTownHallCycleView(
            item.Code,
            item.Label,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Country,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalTownHallCycleView> ToSeasonalTownHallCycleViews(List<SeasonalTownHallCycle> items)
    {
        var views = new List<SeasonalTownHallCycleView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalTownHallCycleView(item));
        }
        return views;
    }

    public static double SumSeasonalTownHallCycleView(SeasonalTownHallCycleView view)
    {
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedLimit + (double)parsedWindowDays + (double)parsedGraceDays;
    }
}
