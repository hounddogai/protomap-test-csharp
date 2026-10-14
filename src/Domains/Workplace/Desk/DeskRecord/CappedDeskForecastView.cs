namespace Acme.Workforce.Workplace.Desk.DeskRecord;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped desk forecast.</summary>
public sealed record CappedDeskForecastView(string Code, string NameText, string JobCode, string Currency, string Version, string FloorValueText, string ContactEmail, string Timezone, string GraceDaysText, string ExternalRef, string StatusLabel)
{
    public static CappedDeskForecastView ToCappedDeskForecastView(CappedDeskForecast item)
    {
        return new CappedDeskForecastView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.JobCode,
            item.Currency,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.Timezone,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedDeskForecastView> ToCappedDeskForecastViews(List<CappedDeskForecast> items)
    {
        var views = new List<CappedDeskForecastView>();
        foreach (var item in items)
        {
            views.Add(ToCappedDeskForecastView(item));
        }
        return views;
    }

    public static double SumCappedDeskForecastView(CappedDeskForecastView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        return (double)parsedVersion + parsedFloorValue + (double)parsedGraceDays;
    }
}
