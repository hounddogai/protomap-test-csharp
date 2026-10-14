namespace Acme.Workforce.Payrollops.Deduction.Agreement;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a deduction metric.</summary>
public sealed record DeductionMetricView(string Code, string Name, string Limit, string Source, string Title, string Grade, string Minimum, string StatusLabel)
{
    public static DeductionMetricView ToDeductionMetricView(DeductionMetric item)
    {
        return new DeductionMetricView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Source, 120),
            item.Title,
            Text.Truncate(item.Grade, 40),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeductionMetricView> ToDeductionMetricViews(List<DeductionMetric> items)
    {
        var views = new List<DeductionMetricView>();
        foreach (var item in items)
        {
            views.Add(ToDeductionMetricView(item));
        }
        return views;
    }

    public static double SumDeductionMetricView(DeductionMetricView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return (double)parsedLimit + parsedMinimum;
    }
}
