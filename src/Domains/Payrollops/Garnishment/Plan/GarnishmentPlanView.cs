namespace Acme.Workforce.Payrollops.Garnishment.Plan;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a garnishment plan.</summary>
public sealed record GarnishmentPlanView(string Code, string Name, string LeaveDays, string DurationMinutes, string SkillText, string Summary, string WindowDaysText, string PhoneText, string Timezone, string StatusLabel)
{
    public static GarnishmentPlanView ToGarnishmentPlanView(GarnishmentPlan item)
    {
        return new GarnishmentPlanView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Skill,
            item.Summary,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Phone,
            Text.Truncate(item.Timezone, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GarnishmentPlanView> ToGarnishmentPlanViews(List<GarnishmentPlan> items)
    {
        var views = new List<GarnishmentPlanView>();
        foreach (var item in items)
        {
            views.Add(ToGarnishmentPlanView(item));
        }
        return views;
    }

    public static double SumGarnishmentPlanView(GarnishmentPlanView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + (double)parsedDurationMinutes + (double)parsedWindowDays;
    }
}
