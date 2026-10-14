namespace Acme.Workforce.Payrollops.TaxForm.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled tax form forecast.</summary>
public sealed record ScheduledTaxFormForecastView(string CodeText, string TitleText, string Fte, string MaximumText, string Timezone, string StatusLabel)
{
    public static ScheduledTaxFormForecastView ToScheduledTaxFormForecastView(ScheduledTaxFormForecast item)
    {
        return new ScheduledTaxFormForecastView(
            Text.Truncate(item.Code, 64),
            item.Title,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Timezone, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledTaxFormForecastView> ToScheduledTaxFormForecastViews(List<ScheduledTaxFormForecast> items)
    {
        var views = new List<ScheduledTaxFormForecastView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledTaxFormForecastView(item));
        }
        return views;
    }

    public static double SumScheduledTaxFormForecastView(ScheduledTaxFormForecastView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        return parsedFte + parsedMaximum;
    }
}
