namespace Acme.Workforce.Engagement.ActionPlan.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled action plan adjustment.</summary>
public sealed record ScheduledActionPlanAdjustmentView(string Code, string NameText, string Cost, string DurationMinutes, string Phone, string StatusLabel)
{
    public static ScheduledActionPlanAdjustmentView ToScheduledActionPlanAdjustmentView(ScheduledActionPlanAdjustment item)
    {
        return new ScheduledActionPlanAdjustmentView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Phone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledActionPlanAdjustmentView> ToScheduledActionPlanAdjustmentViews(List<ScheduledActionPlanAdjustment> items)
    {
        var views = new List<ScheduledActionPlanAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledActionPlanAdjustmentView(item));
        }
        return views;
    }

    public static double SumScheduledActionPlanAdjustmentView(ScheduledActionPlanAdjustmentView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return parsedCost + (double)parsedDurationMinutes;
    }
}
