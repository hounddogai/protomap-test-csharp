namespace Acme.Workforce.Engagement.TownHall.Rule;

using System.Globalization;

/// <summary>How screens and exports show a town hall rule.</summary>
public sealed record TownHallRuleView(string CodeText, string NameText, string RatingText, string Ratio, string Timezone, string JobCode, string Score, string StatusLabel)
{
    public static TownHallRuleView ToTownHallRuleView(TownHallRule item)
    {
        return new TownHallRuleView(
            item.Code,
            item.Name,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Timezone,
            item.JobCode,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TownHallRuleView> ToTownHallRuleViews(List<TownHallRule> items)
    {
        var views = new List<TownHallRuleView>();
        foreach (var item in items)
        {
            views.Add(ToTownHallRuleView(item));
        }
        return views;
    }

    public static double SumTownHallRuleView(TownHallRuleView view)
    {
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedRating + parsedRatio + parsedScore;
    }
}
