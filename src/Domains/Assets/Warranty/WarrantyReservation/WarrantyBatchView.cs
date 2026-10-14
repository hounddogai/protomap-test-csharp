namespace Acme.Workforce.Assets.Warranty.WarrantyReservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a warranty batch.</summary>
public sealed record WarrantyBatchView(string CodeText, string Name, string Score, string Priority, string Retries, string Description, string Limit, string LocaleText, string StartsAt, string Threshold, string StatusLabel)
{
    public static WarrantyBatchView ToWarrantyBatchView(WarrantyBatch item)
    {
        return new WarrantyBatchView(
            item.Code,
            item.Name,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Description, 80),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Locale,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WarrantyBatchView> ToWarrantyBatchViews(List<WarrantyBatch> items)
    {
        var views = new List<WarrantyBatchView>();
        foreach (var item in items)
        {
            views.Add(ToWarrantyBatchView(item));
        }
        return views;
    }

    public static double SumWarrantyBatchView(WarrantyBatchView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return parsedScore + (double)parsedPriority + (double)parsedRetries;
    }
}
