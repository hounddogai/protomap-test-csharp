namespace Acme.Workforce.Talent.Pool.Request;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive pool forecast.</summary>
public sealed record AdaptivePoolForecastView(string Code, string TitleText, string ManagerId, string BatchSizeText, string Label, string TenureMonths, string StatusLabel)
{
    public static AdaptivePoolForecastView ToAdaptivePoolForecastView(AdaptivePoolForecast item)
    {
        return new AdaptivePoolForecastView(
            item.Code,
            Text.Truncate(item.Title, 80),
            Text.Truncate(item.ManagerId, 40),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Label,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptivePoolForecastView> ToAdaptivePoolForecastViews(List<AdaptivePoolForecast> items)
    {
        var views = new List<AdaptivePoolForecastView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptivePoolForecastView(item));
        }
        return views;
    }

    public static double SumAdaptivePoolForecastView(AdaptivePoolForecastView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + (double)parsedTenureMonths;
    }
}
