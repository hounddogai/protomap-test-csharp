namespace Acme.Workforce.Orgdesign.Reorg.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a reorg budget.</summary>
public sealed record ReorgBudgetView(string CodeText, string Title, string Timezone, string LeaveDays, string CreatedAt, string Description, string StatusLabel)
{
    public static ReorgBudgetView ToReorgBudgetView(ReorgBudget item)
    {
        return new ReorgBudgetView(
            item.Code,
            item.Title,
            item.Timezone,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Description, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReorgBudgetView> ToReorgBudgetViews(List<ReorgBudget> items)
    {
        var views = new List<ReorgBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToReorgBudgetView(item));
        }
        return views;
    }

    public static double SumReorgBudgetView(ReorgBudgetView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + (double)parsedCreatedAt;
    }
}
