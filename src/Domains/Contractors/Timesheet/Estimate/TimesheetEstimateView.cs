namespace Acme.Workforce.Contractors.Timesheet.Estimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a timesheet estimate.</summary>
public sealed record TimesheetEstimateView(string CodeText, string LabelText, string Discount, string Quantity, string Name, string CreatedAt, string Budget, string Category, string Department, string Approved, string StartsAt, string StatusLabel)
{
    public static TimesheetEstimateView ToTimesheetEstimateView(TimesheetEstimate item)
    {
        return new TimesheetEstimateView(
            item.Code,
            item.Label,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Name,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Category, 40),
            Text.Truncate(item.Department, 80),
            $"{item.Approved}",
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TimesheetEstimateView> ToTimesheetEstimateViews(List<TimesheetEstimate> items)
    {
        var views = new List<TimesheetEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToTimesheetEstimateView(item));
        }
        return views;
    }

    public static double SumTimesheetEstimateView(TimesheetEstimateView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return parsedDiscount + (double)parsedQuantity + (double)parsedCreatedAt;
    }
}
