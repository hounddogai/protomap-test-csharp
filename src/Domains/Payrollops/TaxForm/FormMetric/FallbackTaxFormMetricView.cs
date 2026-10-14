namespace Acme.Workforce.Payrollops.TaxForm.FormMetric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback tax form metric.</summary>
public sealed record FallbackTaxFormMetricView(string Code, string Label, string Notes, string Locale, string Timezone, string Cost, string StatusLabel)
{
    public static FallbackTaxFormMetricView ToFallbackTaxFormMetricView(FallbackTaxFormMetric item)
    {
        return new FallbackTaxFormMetricView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.Notes,
            Text.Truncate(item.Locale, 64),
            item.Timezone,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackTaxFormMetricView> ToFallbackTaxFormMetricViews(List<FallbackTaxFormMetric> items)
    {
        var views = new List<FallbackTaxFormMetricView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackTaxFormMetricView(item));
        }
        return views;
    }

    public static double SumFallbackTaxFormMetricView(FallbackTaxFormMetricView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return parsedCost;
    }
}
