namespace Acme.Workforce.Orgdesign.Headcount.Schedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted headcount schedule.</summary>
public sealed record WeightedHeadcountScheduleView(string Code, string NameText, string BatchSize, string TeamText, string LeaveDaysText, string EndsAt, string StatusLabel)
{
    public static WeightedHeadcountScheduleView ToWeightedHeadcountScheduleView(WeightedHeadcountSchedule item)
    {
        return new WeightedHeadcountScheduleView(
            item.Code,
            item.Name,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Team, 40),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedHeadcountScheduleView> ToWeightedHeadcountScheduleViews(List<WeightedHeadcountSchedule> items)
    {
        var views = new List<WeightedHeadcountScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedHeadcountScheduleView(item));
        }
        return views;
    }

    public static double SumWeightedHeadcountScheduleView(WeightedHeadcountScheduleView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + (double)parsedLeaveDays + (double)parsedEndsAt;
    }
}
