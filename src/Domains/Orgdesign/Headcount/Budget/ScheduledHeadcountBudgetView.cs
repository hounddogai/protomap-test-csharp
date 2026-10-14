namespace Acme.Workforce.Orgdesign.Headcount.Budget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled headcount budget.</summary>
public sealed record ScheduledHeadcountBudgetView(string Code, string Name, string Revision, string Badge, string Locale, string StatusLabel)
{
    public static ScheduledHeadcountBudgetView ToScheduledHeadcountBudgetView(ScheduledHeadcountBudget item)
    {
        return new ScheduledHeadcountBudgetView(
            item.Code,
            item.Name,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Badge, 120),
            item.Locale,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledHeadcountBudgetView> ToScheduledHeadcountBudgetViews(List<ScheduledHeadcountBudget> items)
    {
        var views = new List<ScheduledHeadcountBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledHeadcountBudgetView(item));
        }
        return views;
    }

    public static double SumScheduledHeadcountBudgetView(ScheduledHeadcountBudgetView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return (double)parsedRevision;
    }
}
