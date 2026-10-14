namespace Acme.Workforce.Assets.Warranty.Budget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a warranty budget.</summary>
public sealed record WarrantyBudgetView(string Code, string Name, string Visible, string LeaveDaysText, string Reviewer, string CreatedAt, string Ceiling, string Source, string WindowDays, string Category, string PriorityText, string StatusLabel)
{
    public static WarrantyBudgetView ToWarrantyBudgetView(WarrantyBudget item)
    {
        return new WarrantyBudgetView(
            item.Code,
            Text.Truncate(item.Name, 64),
            $"{item.Visible}",
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Source,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Category,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WarrantyBudgetView> ToWarrantyBudgetViews(List<WarrantyBudget> items)
    {
        var views = new List<WarrantyBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToWarrantyBudgetView(item));
        }
        return views;
    }

    public static double SumWarrantyBudgetView(WarrantyBudgetView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + (double)parsedCreatedAt + parsedCeiling;
    }
}
