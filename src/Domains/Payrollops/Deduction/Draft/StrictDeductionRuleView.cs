namespace Acme.Workforce.Payrollops.Deduction.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict deduction rule.</summary>
public sealed record StrictDeductionRuleView(string Code, string LabelText, string ManagerId, string ApprovedText, string Skill, string DurationMinutes, string StatusLabel)
{
    public static StrictDeductionRuleView ToStrictDeductionRuleView(StrictDeductionRule item)
    {
        return new StrictDeductionRuleView(
            item.Code,
            item.Label,
            Text.Truncate(item.ManagerId, 80),
            $"{item.Approved}",
            item.Skill,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictDeductionRuleView> ToStrictDeductionRuleViews(List<StrictDeductionRule> items)
    {
        var views = new List<StrictDeductionRuleView>();
        foreach (var item in items)
        {
            views.Add(ToStrictDeductionRuleView(item));
        }
        return views;
    }

    public static double SumStrictDeductionRuleView(StrictDeductionRuleView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes;
    }
}
