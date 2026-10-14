namespace Acme.Workforce.Engagement.TownHall.BatchedHallRule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal town hall metric.</summary>
public sealed record SeasonalTownHallMetricView(string CodeText, string Name, string Locale, string Budget, string StartsAtText, string StatusLabel)
{
    public static SeasonalTownHallMetricView ToSeasonalTownHallMetricView(SeasonalTownHallMetric item)
    {
        return new SeasonalTownHallMetricView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.Locale,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalTownHallMetricView> ToSeasonalTownHallMetricViews(List<SeasonalTownHallMetric> items)
    {
        var views = new List<SeasonalTownHallMetricView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalTownHallMetricView(item));
        }
        return views;
    }

    public static double SumSeasonalTownHallMetricView(SeasonalTownHallMetricView view)
    {
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        return parsedBudget + (double)parsedStartsAt;
    }
}
