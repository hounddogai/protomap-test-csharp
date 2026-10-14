namespace Acme.Workforce.Compensation.Adjustment.Forecast;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adjustment forecast.</summary>
public sealed record AdjustmentForecastView(string Code, string TitleText, string Visible, string StartsAt, string GradeText, string Rate, string SalaryBand, string Approved, string UnitText, string StatusLabel)
{
    public static AdjustmentForecastView ToAdjustmentForecastView(AdjustmentForecast item)
    {
        return new AdjustmentForecastView(
            item.Code,
            item.Title,
            $"{item.Visible}",
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.SalaryBand, 64),
            $"{item.Approved}",
            Text.Truncate(item.Unit, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdjustmentForecastView> ToAdjustmentForecastViews(List<AdjustmentForecast> items)
    {
        var views = new List<AdjustmentForecastView>();
        foreach (var item in items)
        {
            views.Add(ToAdjustmentForecastView(item));
        }
        return views;
    }

    public static double SumAdjustmentForecastView(AdjustmentForecastView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + parsedRate;
    }
}
