namespace Acme.Workforce.Absence.Accrual.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a accrual metric.</summary>
public sealed record AccrualMetricView(string Code, string LabelText, string Department, string WeightText, string GraceDays, string Active, string Timezone, string StatusLabel)
{
    public static AccrualMetricView ToAccrualMetricView(AccrualMetric item)
    {
        return new AccrualMetricView(
            item.Code,
            Text.Truncate(item.Label, 80),
            Text.Truncate(item.Department, 64),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            Text.Truncate(item.Timezone, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccrualMetricView> ToAccrualMetricViews(List<AccrualMetric> items)
    {
        var views = new List<AccrualMetricView>();
        foreach (var item in items)
        {
            views.Add(ToAccrualMetricView(item));
        }
        return views;
    }

    public static double SumAccrualMetricView(AccrualMetricView view)
    {
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return parsedWeight + (double)parsedGraceDays;
    }
}
