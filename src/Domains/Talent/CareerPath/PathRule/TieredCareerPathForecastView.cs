namespace Acme.Workforce.Talent.CareerPath.PathRule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered career path forecast.</summary>
public sealed record TieredCareerPathForecastView(string Code, string Name, string Attempts, string Description, string Region, string Cost, string GraceDaysText, string Approved, string StatusLabel)
{
    public static TieredCareerPathForecastView ToTieredCareerPathForecastView(TieredCareerPathForecast item)
    {
        return new TieredCareerPathForecastView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Description,
            Text.Truncate(item.Region, 64),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredCareerPathForecastView> ToTieredCareerPathForecastViews(List<TieredCareerPathForecast> items)
    {
        var views = new List<TieredCareerPathForecastView>();
        foreach (var item in items)
        {
            views.Add(ToTieredCareerPathForecastView(item));
        }
        return views;
    }

    public static double SumTieredCareerPathForecastView(TieredCareerPathForecastView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + parsedCost + (double)parsedGraceDays;
    }
}
