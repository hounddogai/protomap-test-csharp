namespace Acme.Workforce.Assets.Refresh.Forecast;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a refresh forecast.</summary>
public sealed record RefreshForecastView(string Code, string Title, string Channel, string Ratio, string RetriesText, string Sequence, string Version, string SummaryText, string StatusLabel)
{
    public static RefreshForecastView ToRefreshForecastView(RefreshForecast item)
    {
        return new RefreshForecastView(
            Text.Truncate(item.Code, 120),
            item.Title,
            item.Channel,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Summary,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RefreshForecastView> ToRefreshForecastViews(List<RefreshForecast> items)
    {
        var views = new List<RefreshForecastView>();
        foreach (var item in items)
        {
            views.Add(ToRefreshForecastView(item));
        }
        return views;
    }

    public static double SumRefreshForecastView(RefreshForecastView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return parsedRatio + (double)parsedRetries + (double)parsedSequence;
    }
}
