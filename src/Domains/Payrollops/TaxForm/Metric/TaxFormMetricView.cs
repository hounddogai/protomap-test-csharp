namespace Acme.Workforce.Payrollops.TaxForm.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tax form metric.</summary>
public sealed record TaxFormMetricView(string Code, string LabelText, string DurationMinutesText, string Currency, string Locale, string StatusLabel)
{
    public static TaxFormMetricView ToTaxFormMetricView(TaxFormMetric item)
    {
        return new TaxFormMetricView(
            item.Code,
            item.Label,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Currency,
            Text.Truncate(item.Locale, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TaxFormMetricView> ToTaxFormMetricViews(List<TaxFormMetric> items)
    {
        var views = new List<TaxFormMetricView>();
        foreach (var item in items)
        {
            views.Add(ToTaxFormMetricView(item));
        }
        return views;
    }

    public static double SumTaxFormMetricView(TaxFormMetricView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes;
    }
}
