namespace Acme.Workforce.Payrollops.Deduction.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped deduction group.</summary>
public sealed record CappedDeductionGroupView(string Code, string Name, string NotesText, string PriorityText, string SkillText, string Region, string Retries, string StatusLabel)
{
    public static CappedDeductionGroupView ToCappedDeductionGroupView(CappedDeductionGroup item)
    {
        return new CappedDeductionGroupView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.Notes,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Skill,
            item.Region,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedDeductionGroupView> ToCappedDeductionGroupViews(List<CappedDeductionGroup> items)
    {
        var views = new List<CappedDeductionGroupView>();
        foreach (var item in items)
        {
            views.Add(ToCappedDeductionGroupView(item));
        }
        return views;
    }

    public static double SumCappedDeductionGroupView(CappedDeductionGroupView view)
    {
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedPriority + (double)parsedRetries;
    }
}
