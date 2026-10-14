namespace Acme.Workforce.Talent.SuccessionPlan.PlanAgreement;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a succession plan allocation.</summary>
public sealed record SuccessionPlanAllocationView(string CodeText, string Label, string Visible, string Retries, string ExternalRef, string LimitText, string Approved, string StartsAt, string WindowDaysText, string Skill, string StatusLabel)
{
    public static SuccessionPlanAllocationView ToSuccessionPlanAllocationView(SuccessionPlanAllocation item)
    {
        return new SuccessionPlanAllocationView(
            Text.Truncate(item.Code, 120),
            item.Label,
            $"{item.Visible}",
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Skill,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SuccessionPlanAllocationView> ToSuccessionPlanAllocationViews(List<SuccessionPlanAllocation> items)
    {
        var views = new List<SuccessionPlanAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToSuccessionPlanAllocationView(item));
        }
        return views;
    }

    public static double SumSuccessionPlanAllocationView(SuccessionPlanAllocationView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return (double)parsedRetries + (double)parsedLimit + (double)parsedStartsAt;
    }
}
