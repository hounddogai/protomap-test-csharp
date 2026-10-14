namespace Acme.Workforce.Engagement.TownHall.Review;

using System.Globalization;

/// <summary>How screens and exports show a town hall forecast.</summary>
public sealed record TownHallForecastView(string Code, string Title, string Budget, string ExternalRefText, string Checksum, string Fte, string ScoreText, string StatusLabel)
{
    public static TownHallForecastView ToTownHallForecastView(TownHallForecast item)
    {
        return new TownHallForecastView(
            item.Code,
            item.Title,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.Checksum,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TownHallForecastView> ToTownHallForecastViews(List<TownHallForecast> items)
    {
        var views = new List<TownHallForecastView>();
        foreach (var item in items)
        {
            views.Add(ToTownHallForecastView(item));
        }
        return views;
    }

    public static double SumTownHallForecastView(TownHallForecastView view)
    {
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return parsedBudget + parsedFte + parsedScore;
    }
}
