namespace Acme.Workforce.Contractors.Vendor.VendorTemplate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched vendor template.</summary>
public sealed record BatchedVendorTemplateView(string Code, string Title, string SalaryBand, string RevisionText, string Margin, string CeilingText, string AttemptsText, string BatchSizeText, string Quantity, string StatusLabel)
{
    public static BatchedVendorTemplateView ToBatchedVendorTemplateView(BatchedVendorTemplate item)
    {
        return new BatchedVendorTemplateView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Title, 120),
            item.SalaryBand,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedVendorTemplateView> ToBatchedVendorTemplateViews(List<BatchedVendorTemplate> items)
    {
        var views = new List<BatchedVendorTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedVendorTemplateView(item));
        }
        return views;
    }

    public static double SumBatchedVendorTemplateView(BatchedVendorTemplateView view)
    {
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        return (double)parsedRevision + parsedMargin + parsedCeiling;
    }
}
