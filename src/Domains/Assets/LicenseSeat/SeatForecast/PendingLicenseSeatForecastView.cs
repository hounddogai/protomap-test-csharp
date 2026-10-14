namespace Acme.Workforce.Assets.LicenseSeat.SeatForecast;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending license seat forecast.</summary>
public sealed record PendingLicenseSeatForecastView(string CodeText, string Title, string CostCenterText, string Budget, string QuantityText, string TenureMonthsText, string Skill, string CreatedAt, string GraceDaysText, string StatusLabel)
{
    public static PendingLicenseSeatForecastView ToPendingLicenseSeatForecastView(PendingLicenseSeatForecast item)
    {
        return new PendingLicenseSeatForecastView(
            Text.Truncate(item.Code, 64),
            item.Title,
            Text.Truncate(item.CostCenter, 64),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Skill,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingLicenseSeatForecastView> ToPendingLicenseSeatForecastViews(List<PendingLicenseSeatForecast> items)
    {
        var views = new List<PendingLicenseSeatForecastView>();
        foreach (var item in items)
        {
            views.Add(ToPendingLicenseSeatForecastView(item));
        }
        return views;
    }

    public static double SumPendingLicenseSeatForecastView(PendingLicenseSeatForecastView view)
    {
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        return parsedBudget + (double)parsedQuantity + (double)parsedTenureMonths;
    }
}
