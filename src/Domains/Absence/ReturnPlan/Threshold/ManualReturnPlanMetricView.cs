namespace Acme.Workforce.Absence.ReturnPlan.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual return plan metric.</summary>
public sealed record ManualReturnPlanMetricView(string Code, string NameText, string SegmentText, string ExpiresAtText, string OwnerId, string Fte, string Discount, string StatusLabel)
{
    public static ManualReturnPlanMetricView ToManualReturnPlanMetricView(ManualReturnPlanMetric item)
    {
        return new ManualReturnPlanMetricView(
            item.Code,
            item.Name,
            Text.Truncate(item.Segment, 40),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualReturnPlanMetricView> ToManualReturnPlanMetricViews(List<ManualReturnPlanMetric> items)
    {
        var views = new List<ManualReturnPlanMetricView>();
        foreach (var item in items)
        {
            views.Add(ToManualReturnPlanMetricView(item));
        }
        return views;
    }

    public static double SumManualReturnPlanMetricView(ManualReturnPlanMetricView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + parsedFte + parsedDiscount;
    }
}
