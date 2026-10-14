namespace Acme.Workforce.Compliance.Audit.Batch;

using System.Globalization;

/// <summary>How screens and exports show a audit batch.</summary>
public sealed record AuditBatchView(string Code, string Title, string DiscountText, string RetriesText, string BadgeText, string CreatedAtText, string StatusLabel)
{
    public static AuditBatchView ToAuditBatchView(AuditBatch item)
    {
        return new AuditBatchView(
            item.Code,
            item.Title,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Badge,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AuditBatchView> ToAuditBatchViews(List<AuditBatch> items)
    {
        var views = new List<AuditBatchView>();
        foreach (var item in items)
        {
            views.Add(ToAuditBatchView(item));
        }
        return views;
    }

    public static double SumAuditBatchView(AuditBatchView view)
    {
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        return parsedDiscount + (double)parsedRetries + (double)parsedCreatedAt;
    }
}
