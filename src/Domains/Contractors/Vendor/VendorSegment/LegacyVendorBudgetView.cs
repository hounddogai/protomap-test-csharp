namespace Acme.Workforce.Contractors.Vendor.VendorSegment;

using System.Globalization;

/// <summary>How screens and exports show a legacy vendor budget.</summary>
public sealed record LegacyVendorBudgetView(string Code, string Name, string Amount, string Weight, string Region, string Unit, string StatusLabel)
{
    public static LegacyVendorBudgetView ToLegacyVendorBudgetView(LegacyVendorBudget item)
    {
        return new LegacyVendorBudgetView(
            item.Code,
            item.Name,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Region,
            item.Unit,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyVendorBudgetView> ToLegacyVendorBudgetViews(List<LegacyVendorBudget> items)
    {
        var views = new List<LegacyVendorBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyVendorBudgetView(item));
        }
        return views;
    }

    public static double SumLegacyVendorBudgetView(LegacyVendorBudgetView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return parsedAmount + parsedWeight;
    }
}
