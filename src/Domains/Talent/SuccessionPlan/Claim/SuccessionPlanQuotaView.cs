namespace Acme.Workforce.Talent.SuccessionPlan.Claim;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a succession plan quota.</summary>
public sealed record SuccessionPlanQuotaView(string CodeText, string TitleText, string DurationMinutesText, string Reviewer, string LeaveDays, string Department, string CeilingText, string Timezone, string StatusLabel)
{
    public static SuccessionPlanQuotaView ToSuccessionPlanQuotaView(SuccessionPlanQuota item)
    {
        return new SuccessionPlanQuotaView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Department,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Timezone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SuccessionPlanQuotaView> ToSuccessionPlanQuotaViews(List<SuccessionPlanQuota> items)
    {
        var views = new List<SuccessionPlanQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToSuccessionPlanQuotaView(item));
        }
        return views;
    }

    public static double SumSuccessionPlanQuotaView(SuccessionPlanQuotaView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + (double)parsedLeaveDays + parsedCeiling;
    }
}
