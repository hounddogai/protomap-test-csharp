namespace Acme.Workforce.Compensation.Adjustment.Schedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adjustment variance.</summary>
public sealed record AdjustmentVarianceView(string Code, string Label, string GradeText, string Currency, string SegmentText, string Channel, string DurationMinutes, string Archived, string Timezone, string DepartmentText, string StatusLabel)
{
    public static AdjustmentVarianceView ToAdjustmentVarianceView(AdjustmentVariance item)
    {
        return new AdjustmentVarianceView(
            item.Code,
            item.Label,
            item.Grade,
            item.Currency,
            item.Segment,
            Text.Truncate(item.Channel, 40),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Timezone,
            item.Department,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdjustmentVarianceView> ToAdjustmentVarianceViews(List<AdjustmentVariance> items)
    {
        var views = new List<AdjustmentVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToAdjustmentVarianceView(item));
        }
        return views;
    }

    public static double SumAdjustmentVarianceView(AdjustmentVarianceView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes;
    }
}
