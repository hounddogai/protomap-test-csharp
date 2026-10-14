namespace Acme.Workforce.Talent.Mobility.Forecast;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a mobility forecast.</summary>
public sealed record MobilityForecastView(string Code, string Name, string QuantityText, string ContactEmail, string Currency, string AmountText, string HoursText, string CostCenter, string PhoneText, string WeightText, string EmployeeId, string StatusLabel)
{
    public static MobilityForecastView ToMobilityForecastView(MobilityForecast item)
    {
        return new MobilityForecastView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.Currency,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.CostCenter,
            Text.Truncate(item.Phone, 64),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.EmployeeId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MobilityForecastView> ToMobilityForecastViews(List<MobilityForecast> items)
    {
        var views = new List<MobilityForecastView>();
        foreach (var item in items)
        {
            views.Add(ToMobilityForecastView(item));
        }
        return views;
    }

    public static double SumMobilityForecastView(MobilityForecastView view)
    {
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + parsedAmount + parsedHours;
    }
}
