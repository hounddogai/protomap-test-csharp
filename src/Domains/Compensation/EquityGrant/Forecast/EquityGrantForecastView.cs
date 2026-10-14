namespace Acme.Workforce.Compensation.EquityGrant.Forecast;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a equity grant forecast.</summary>
public sealed record EquityGrantForecastView(string Code, string Title, string CostCenterText, string ManagerId, string RateText, string TimezoneText, string Limit, string Unit, string Reviewer, string ExpiresAt, string Location, string StatusLabel)
{
    public static EquityGrantForecastView ToEquityGrantForecastView(EquityGrantForecast item)
    {
        return new EquityGrantForecastView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Title, 120),
            item.CostCenter,
            Text.Truncate(item.ManagerId, 64),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Timezone,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Unit,
            item.Reviewer,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Location,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EquityGrantForecastView> ToEquityGrantForecastViews(List<EquityGrantForecast> items)
    {
        var views = new List<EquityGrantForecastView>();
        foreach (var item in items)
        {
            views.Add(ToEquityGrantForecastView(item));
        }
        return views;
    }

    public static double SumEquityGrantForecastView(EquityGrantForecastView view)
    {
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedLimit + (double)parsedExpiresAt;
    }
}
