namespace Acme.Workforce.Assets.Laptop.Forecast;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a laptop forecast.</summary>
public sealed record LaptopForecastView(string Code, string Title, string LabelText, string Maximum, string Threshold, string PinnedText, string Summary, string Cost, string StatusLabel)
{
    public static LaptopForecastView ToLaptopForecastView(LaptopForecast item)
    {
        return new LaptopForecastView(
            Text.Truncate(item.Code, 64),
            item.Title,
            item.Label,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            Text.Truncate(item.Summary, 80),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LaptopForecastView> ToLaptopForecastViews(List<LaptopForecast> items)
    {
        var views = new List<LaptopForecastView>();
        foreach (var item in items)
        {
            views.Add(ToLaptopForecastView(item));
        }
        return views;
    }

    public static double SumLaptopForecastView(LaptopForecastView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return parsedMaximum + parsedThreshold + parsedCost;
    }
}
