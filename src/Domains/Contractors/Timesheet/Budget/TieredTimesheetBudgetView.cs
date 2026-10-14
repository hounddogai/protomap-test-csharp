namespace Acme.Workforce.Contractors.Timesheet.Budget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered timesheet budget.</summary>
public sealed record TieredTimesheetBudgetView(string Code, string NameText, string Department, string CeilingText, string Fte, string Minimum, string StatusLabel)
{
    public static TieredTimesheetBudgetView ToTieredTimesheetBudgetView(TieredTimesheetBudget item)
    {
        return new TieredTimesheetBudgetView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.Department,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredTimesheetBudgetView> ToTieredTimesheetBudgetViews(List<TieredTimesheetBudget> items)
    {
        var views = new List<TieredTimesheetBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToTieredTimesheetBudgetView(item));
        }
        return views;
    }

    public static double SumTieredTimesheetBudgetView(TieredTimesheetBudgetView view)
    {
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return parsedCeiling + parsedFte + parsedMinimum;
    }
}
