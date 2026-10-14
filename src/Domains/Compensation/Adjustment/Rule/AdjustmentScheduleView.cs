namespace Acme.Workforce.Compensation.Adjustment.Rule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adjustment schedule.</summary>
public sealed record AdjustmentScheduleView(string Code, string Label, string SequenceText, string SummaryText, string GraceDaysText, string ManagerId, string Discount, string JobCode, string Region, string CategoryText, string ReviewerText, string StatusLabel)
{
    public static AdjustmentScheduleView ToAdjustmentScheduleView(AdjustmentSchedule item)
    {
        return new AdjustmentScheduleView(
            Text.Truncate(item.Code, 120),
            item.Label,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Summary,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ManagerId, 120),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.JobCode, 80),
            item.Region,
            item.Category,
            item.Reviewer,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdjustmentScheduleView> ToAdjustmentScheduleViews(List<AdjustmentSchedule> items)
    {
        var views = new List<AdjustmentScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToAdjustmentScheduleView(item));
        }
        return views;
    }

    public static double SumAdjustmentScheduleView(AdjustmentScheduleView view)
    {
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return (double)parsedSequence + (double)parsedGraceDays + parsedDiscount;
    }
}
