namespace Acme.Workforce.Compensation.PayBand.Forecast;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled pay band forecast.</summary>
public sealed record ScheduledPayBandForecastView(string Code, string LabelText, string TenureMonthsText, string TeamText, string NameText, string StatusLabel)
{
    public static ScheduledPayBandForecastView ToScheduledPayBandForecastView(ScheduledPayBandForecast item)
    {
        return new ScheduledPayBandForecastView(
            item.Code,
            item.Label,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Team,
            Text.Truncate(item.Name, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledPayBandForecastView> ToScheduledPayBandForecastViews(List<ScheduledPayBandForecast> items)
    {
        var views = new List<ScheduledPayBandForecastView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledPayBandForecastView(item));
        }
        return views;
    }

    public static double SumScheduledPayBandForecastView(ScheduledPayBandForecastView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths;
    }
}
