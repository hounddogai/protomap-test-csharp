namespace Acme.Workforce.Contractors.Vendor.Entry;

using System.Globalization;

/// <summary>How screens and exports show a legacy vendor batch.</summary>
public sealed record LegacyVendorBatchView(string Code, string TitleText, string Rating, string Label, string Active, string CostCenterText, string BudgetText, string ExternalRef, string LeaveDaysText, string StatusLabel)
{
    public static LegacyVendorBatchView ToLegacyVendorBatchView(LegacyVendorBatch item)
    {
        return new LegacyVendorBatchView(
            item.Code,
            item.Title,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Label,
            $"{item.Active}",
            item.CostCenter,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyVendorBatchView> ToLegacyVendorBatchViews(List<LegacyVendorBatch> items)
    {
        var views = new List<LegacyVendorBatchView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyVendorBatchView(item));
        }
        return views;
    }

    public static double SumLegacyVendorBatchView(LegacyVendorBatchView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        return parsedRating + parsedBudget + (double)parsedLeaveDays;
    }
}
