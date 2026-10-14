namespace Acme.Workforce.Engagement.ActionPlan.Policy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default action plan policy.</summary>
public sealed record DefaultActionPlanPolicyView(string Code, string Label, string Visible, string DurationMinutes, string Ratio, string Currency, string CategoryText, string HoursText, string Fte, string NotesText, string PriorityText, string StatusLabel)
{
    public static DefaultActionPlanPolicyView ToDefaultActionPlanPolicyView(DefaultActionPlanPolicy item)
    {
        return new DefaultActionPlanPolicyView(
            item.Code,
            Text.Truncate(item.Label, 80),
            $"{item.Visible}",
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            Text.Truncate(item.Category, 40),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Notes,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultActionPlanPolicyView> ToDefaultActionPlanPolicyViews(List<DefaultActionPlanPolicy> items)
    {
        var views = new List<DefaultActionPlanPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultActionPlanPolicyView(item));
        }
        return views;
    }

    public static double SumDefaultActionPlanPolicyView(DefaultActionPlanPolicyView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + parsedRatio + parsedHours;
    }
}
