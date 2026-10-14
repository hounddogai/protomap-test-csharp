namespace Acme.Workforce.Orgdesign.Headcount.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a headcount forecast.</summary>
public sealed record HeadcountForecastView(string Code, string Name, string WindowDays, string Reviewer, string Fte, string StartsAt, string MarginText, string Cost, string SalaryBandText, string Minimum, string StatusLabel)
{
    public static HeadcountForecastView ToHeadcountForecastView(HeadcountForecast item)
    {
        return new HeadcountForecastView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.SalaryBand, 40),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HeadcountForecastView> ToHeadcountForecastViews(List<HeadcountForecast> items)
    {
        var views = new List<HeadcountForecastView>();
        foreach (var item in items)
        {
            views.Add(ToHeadcountForecastView(item));
        }
        return views;
    }

    public static double SumHeadcountForecastView(HeadcountForecastView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + parsedFte + (double)parsedStartsAt;
    }
}
