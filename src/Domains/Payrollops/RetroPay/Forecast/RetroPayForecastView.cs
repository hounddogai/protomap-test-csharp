namespace Acme.Workforce.Payrollops.RetroPay.Forecast;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a retro pay forecast.</summary>
public sealed record RetroPayForecastView(string Code, string Title, string Description, string LeaveDays, string Ceiling, string RegionText, string QuantityText, string StatusLabel)
{
    public static RetroPayForecastView ToRetroPayForecastView(RetroPayForecast item)
    {
        return new RetroPayForecastView(
            item.Code,
            item.Title,
            Text.Truncate(item.Description, 80),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Region,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RetroPayForecastView> ToRetroPayForecastViews(List<RetroPayForecast> items)
    {
        var views = new List<RetroPayForecastView>();
        foreach (var item in items)
        {
            views.Add(ToRetroPayForecastView(item));
        }
        return views;
    }

    public static double SumRetroPayForecastView(RetroPayForecastView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + parsedCeiling + (double)parsedQuantity;
    }
}
