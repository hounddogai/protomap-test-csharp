namespace Acme.Workforce.Contractors.Engagement.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a engagement forecast.</summary>
public sealed record EngagementForecastView(string Code, string Label, string Hours, string Archived, string ExpiresAtText, string ManagerIdText, string StatusLabel)
{
    public static EngagementForecastView ToEngagementForecastView(EngagementForecast item)
    {
        return new EngagementForecastView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ManagerId, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EngagementForecastView> ToEngagementForecastViews(List<EngagementForecast> items)
    {
        var views = new List<EngagementForecastView>();
        foreach (var item in items)
        {
            views.Add(ToEngagementForecastView(item));
        }
        return views;
    }

    public static double SumEngagementForecastView(EngagementForecastView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        return parsedHours + (double)parsedExpiresAt;
    }
}
