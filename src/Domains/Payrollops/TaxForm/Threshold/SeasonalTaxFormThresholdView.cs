namespace Acme.Workforce.Payrollops.TaxForm.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal tax form threshold.</summary>
public sealed record SeasonalTaxFormThresholdView(string Code, string Label, string Margin, string DepartmentText, string JobCode, string Threshold, string Archived, string Currency, string StatusLabel)
{
    public static SeasonalTaxFormThresholdView ToSeasonalTaxFormThresholdView(SeasonalTaxFormThreshold item)
    {
        return new SeasonalTaxFormThresholdView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Department,
            Text.Truncate(item.JobCode, 40),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Currency,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalTaxFormThresholdView> ToSeasonalTaxFormThresholdViews(List<SeasonalTaxFormThreshold> items)
    {
        var views = new List<SeasonalTaxFormThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalTaxFormThresholdView(item));
        }
        return views;
    }

    public static double SumSeasonalTaxFormThresholdView(SeasonalTaxFormThresholdView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return parsedMargin + parsedThreshold;
    }
}
