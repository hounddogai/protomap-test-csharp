namespace Acme.Workforce.Contractors.Vendor.Budget;

using System.Globalization;

/// <summary>How screens and exports show a vendor budget.</summary>
public sealed record VendorBudgetView(string Code, string Name, string WindowDaysText, string GraceDaysText, string Limit, string StatusLabel)
{
    public static VendorBudgetView ToVendorBudgetView(VendorBudget item)
    {
        return new VendorBudgetView(
            item.Code,
            item.Name,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VendorBudgetView> ToVendorBudgetViews(List<VendorBudget> items)
    {
        var views = new List<VendorBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToVendorBudgetView(item));
        }
        return views;
    }

    public static double SumVendorBudgetView(VendorBudgetView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + (double)parsedGraceDays + (double)parsedLimit;
    }
}
