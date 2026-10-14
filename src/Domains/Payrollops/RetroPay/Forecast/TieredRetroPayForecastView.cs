namespace Acme.Workforce.Payrollops.RetroPay.Forecast;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered retro pay forecast.</summary>
public sealed record TieredRetroPayForecastView(string Code, string Name, string ActiveText, string GraceDays, string CreatedAt, string LeaveDays, string Phone, string Rate, string EmployeeId, string StatusLabel)
{
    public static TieredRetroPayForecastView ToTieredRetroPayForecastView(TieredRetroPayForecast item)
    {
        return new TieredRetroPayForecastView(
            item.Code,
            Text.Truncate(item.Name, 80),
            $"{item.Active}",
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Phone, 64),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.EmployeeId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredRetroPayForecastView> ToTieredRetroPayForecastViews(List<TieredRetroPayForecast> items)
    {
        var views = new List<TieredRetroPayForecastView>();
        foreach (var item in items)
        {
            views.Add(ToTieredRetroPayForecastView(item));
        }
        return views;
    }

    public static double SumTieredRetroPayForecastView(TieredRetroPayForecastView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + (double)parsedCreatedAt + (double)parsedLeaveDays;
    }
}
