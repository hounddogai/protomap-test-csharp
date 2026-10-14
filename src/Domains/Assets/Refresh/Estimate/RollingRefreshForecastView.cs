namespace Acme.Workforce.Assets.Refresh.Estimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling refresh forecast.</summary>
public sealed record RollingRefreshForecastView(string Code, string Title, string Department, string LimitText, string ExpiresAt, string CreatedAtText, string StatusLabel)
{
    public static RollingRefreshForecastView ToRollingRefreshForecastView(RollingRefreshForecast item)
    {
        return new RollingRefreshForecastView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Title, 64),
            Text.Truncate(item.Department, 120),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingRefreshForecastView> ToRollingRefreshForecastViews(List<RollingRefreshForecast> items)
    {
        var views = new List<RollingRefreshForecastView>();
        foreach (var item in items)
        {
            views.Add(ToRollingRefreshForecastView(item));
        }
        return views;
    }

    public static double SumRollingRefreshForecastView(RollingRefreshForecastView view)
    {
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        return (double)parsedLimit + (double)parsedExpiresAt + (double)parsedCreatedAt;
    }
}
