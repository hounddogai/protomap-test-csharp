namespace Acme.Workforce.Absence.Accrual.Slot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a accrual forecast.</summary>
public sealed record AccrualForecastView(string Code, string Name, string JobCodeText, string Margin, string Grade, string Amount, string FloorValueText, string SalaryBand, string StatusLabel)
{
    public static AccrualForecastView ToAccrualForecastView(AccrualForecast item)
    {
        return new AccrualForecastView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.JobCode,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Grade,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.SalaryBand,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccrualForecastView> ToAccrualForecastViews(List<AccrualForecast> items)
    {
        var views = new List<AccrualForecastView>();
        foreach (var item in items)
        {
            views.Add(ToAccrualForecastView(item));
        }
        return views;
    }

    public static double SumAccrualForecastView(AccrualForecastView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        return parsedMargin + parsedAmount + parsedFloorValue;
    }
}
