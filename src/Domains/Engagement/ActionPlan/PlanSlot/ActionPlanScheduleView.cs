namespace Acme.Workforce.Engagement.ActionPlan.PlanSlot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a action plan schedule.</summary>
public sealed record ActionPlanScheduleView(string CodeText, string Name, string Score, string UnitText, string LeaveDays, string Retries, string StatusLabel)
{
    public static ActionPlanScheduleView ToActionPlanScheduleView(ActionPlanSchedule item)
    {
        return new ActionPlanScheduleView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Unit, 80),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ActionPlanScheduleView> ToActionPlanScheduleViews(List<ActionPlanSchedule> items)
    {
        var views = new List<ActionPlanScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToActionPlanScheduleView(item));
        }
        return views;
    }

    public static double SumActionPlanScheduleView(ActionPlanScheduleView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return parsedScore + (double)parsedLeaveDays + (double)parsedRetries;
    }
}
